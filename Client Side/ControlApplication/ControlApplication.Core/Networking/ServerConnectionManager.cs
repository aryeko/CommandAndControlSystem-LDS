using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ControlApplication.Core.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ControlApplication.Core.Networking
{
    /// <summary>
    /// A class that manages the Server connection and creates a single instance of <see cref="ServerConnectionManager"/> (Singelton)
    /// The class provides a detailed API for sessioned communication with the Server.
    /// </summary>
    internal class ServerConnectionManager : INtServerApi, IDisposable
    {
        /// <summary>
        /// The Remote Server Path
        /// </summary>
        private readonly Uri RemoteServerPath = new Uri("https://127.0.0.1:5000/");

        /// <summary>
        /// WebClient for raw network communication
        /// </summary>
        private CookieAwareWebClient WebClient { get; }

        /// <summary>
        /// Lock object for instance safe access (mutex)
        /// </summary>
        private static readonly object RestOperationLock = new object();

        /// <summary>
        /// Gets the single instance of <see cref="ServerConnectionManager"/> 
        /// </summary>
        /// <returns>Returns single instance of <see cref="ServerConnectionManager"/> as long as the app is running</returns>
        public static ServerConnectionManager Instance { get; } = new ServerConnectionManager();

        /// <summary>
        /// Private constructor which will avoid from an external class to create another instance of <see cref="ServerConnectionManager"/>
        /// </summary>
        private ServerConnectionManager()
        {
            WebClient = new CookieAwareWebClient();
        }

        public event EventHandler<DetectionAddedEventArgs> DetectionAdded;

        /// <summary>
        /// User login authentication 
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Returns whether the user is authorizes or not</returns>
        public bool Login(string username, string password)
        {
            var postData = new NameValueCollection
            {
                { "username", username },
                { "password", password }
            };

            var response = PostToDb("login", postData);
            return response.Contains("SUCCESS");
        }

        /// <summary>
        /// gets materials from the database using server's RESTful API
        /// </summary>
        /// <param name="materialId">Optional: filter by material ID</param>
        /// <param name="name">Optional: filter by material name</param>
        /// <returns></returns>
        public List<Material> GetMaterial(string materialId = "", string name = "")
        {
            var materials = new List<Material>();
            dynamic response = GetObject("material");

            foreach (dynamic obj in response)
            {
                materials.Add(ServerObjectConverter.ConvertMaterial(obj));
            }

            return materials;
        }

        /// <summary>
        /// gets all areas or a specific area from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        public List<Area> GetArea(string areaId = "")
        {
            var areas = new List<Area>();
            dynamic response = GetObject("area");

            foreach (dynamic obj in response)
            {
                areas.Add(ServerObjectConverter.ConvertArea(obj));
            }

            return areas;
        }

        /// <summary>
        /// gets a G-Scan from the database using server's RESTful API
        /// </summary>
        /// <param name="gscanSn">Filter the search by G-Scan uniqe ID</param>
        /// <param name="gscanId"></param>
        /// <returns></returns>
        public List<Gscan> GetGscan(string gscanSn = "", string gscanId = "")
        {
            var gscans = new List<Gscan>();
            var response = GetObject("gscan");

            foreach (dynamic obj in response)
            {
                gscans.Add(ServerObjectConverter.ConvertGscan(obj));
            }

            return gscans;
        }

        /// <summary>
        /// gets all detections from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        public List<Detection> GetDetections(string areaId = "", string detectionId = "")
        {
            return null;
        }

        /// <summary>
        /// gets a raman from the database using server's RESTful API
        /// </summary>
        /// <param name="ramanOutput"></param>
        /// <returns></returns>
        public string GetRaman(string ramanOutput)
        {
            if (ramanOutput.Equals("no raman"))
                return "";

            //TODO: handle a real raman link
            //dynamic response = GetObject("raman", "_id", ramanOutput);
            //return response[0]._id.ToString();
            return "";
        }

        /// <summary>
        /// Adds a detection to the database using server's RESTful API
        /// </summary>
        /// <param name="detection">A detection to add</param>
        /// <param name="idsDictionary">Dictionary of IDs</param>
        public void AddDetection(Detection detection, Dictionary<string,string> idsDictionary)
        {
            var postData = new NameValueCollection
            {
                { "user_id", idsDictionary["UserId"] },
                { "material_id", idsDictionary["MaterialId"] },
                { "area_id", idsDictionary["AreaId"] },
                { "gscan_id", idsDictionary["GscanId"] },
                { "suspect_id", detection.SuspectId },
                { "raman_id", detection.RamanGraph },
                { "plate_number", detection.SuspectPlateId },
                { "location", detection.Position.ToString() },
                { "date_time", detection.DateTimeOfDetection.ToString("G", CultureInfo.InvariantCulture) }
            };

            PostToDb("detection", postData);
            DetectionAdded?.Invoke(this, new DetectionAddedEventArgs(detection));
        }

        /// <summary>
        /// Adds a new area to the database using server's RESTful API
        /// </summary>
        /// <param name="newArea">An area to add</param>
        public void AddArea(Area newArea)
        {
            var postData = new NameValueCollection
            {
                { "area_type", newArea.AreaType.ToString() },
                { "root_location", newArea.RootLocation.ToString() },
                { "radius", newArea.Radius.ToString() }
            };

            newArea.DatabaseId = PostToDb("area", postData);
        }

        /// <summary>
        /// Adds a new Gscan to the database using server's RESTfull API
        /// </summary>
        /// <param name="newGscan">a Gscan to add</param>
        public void AddGscan(Gscan newGscan)
        {
            var postData = new NameValueCollection
            {
                { "gscan_sn", newGscan.PhysicalAddress.ToString() },
                { "owned_unit_id", newGscan.OwnedUnitDatabaseId }
            };

            PostToDb("gscan", postData);
        }

        /// <summary>
        /// Adds a materials combination to the database using server's RESfull API
        /// </summary>
        /// <param name="combination">the combination to add</param>
        public void AddMaterialsCombinationAlert(Combination combination)
        {
            var materialsIds = combination.CombinationMaterialsList.Select(m => m.DatabaseId);

            var postData = new NameValueCollection
            {
                { "materials_list", string.Join(",", materialsIds) },
                { "alert_name", combination.AlertName }
            };

            PostToDb("materials_combination", postData);
        }

        /// <summary>
        /// Get materials combination alert from the database using server's RESfull API
        /// </summary>
        /// <param name="combinationId">the combination ID to filter, gets all combinations if empty</param>
        /// <returns></returns>
        public List<Combination> GetMaterialsCombinationsAlerts(string combinationId = "")
        {
            return null;
        }

        public void CacheCrucialObjects(){}

        public List<Alert> GetAlerts()
        {
            return null;
        }

        public void AddAlert(Alert alert)
        {
            var postData = BuildAlertValueCollection(alert);
            PostToDb("alert", postData);
        }

        public void UpdateAlert(Alert alert)
        {
            var postData = BuildAlertValueCollection(alert);
            postData.Add("_id", alert.DatabaseId);
            PostToDb("alert", postData);
        }

        private static NameValueCollection BuildAlertValueCollection(Alert alert)
        {
            var detectionsIds = alert.Detections.Select(d => d.DatabaseId);

            return new NameValueCollection
            {
                { "detections_list", string.Join(",", detectionsIds) },
                { "alert_name", alert.AlertName },
                { "area_id", alert.Area.DatabaseId },
                { "is_dirty", alert.IsDirty ? "1":"0"},
                { "date_time", alert.AlertTime.ToString("G", CultureInfo.InvariantCulture) }
            };
        }

        /// <summary>
        /// Gets data from the DB and handles WebExeptions
        /// </summary>
        /// <param name="uriPath">URI Path</param>
        /// <param name="key">The key to get value from</param>
        /// <param name="value">The key value</param>
        /// <returns>Response from DB</returns>
        public dynamic GetObject(string uriPath, string key = "", string value = "")
        {
            Logger.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] is trying to achieve GET REST lock for {uriPath}", GetType().Name);
            lock (RestOperationLock)
            {
                Logger.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] has achieved GET REST lock for {uriPath}", GetType().Name);
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    WebClient.QueryString = new NameValueCollection
                {
                    {key, value}
                };
                }
                else
                {
                    WebClient.QueryString = new NameValueCollection();
                }

                try
                {
                    var response = WebClient.DownloadString(new Uri(RemoteServerPath, uriPath));
                    Logger.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] is releasing GET REST lock for {uriPath}", GetType().Name);
                    return JsonConvert.DeserializeObject(response);
                }
                catch (WebException)
                {
                    MessageBox.Show("Your session expired! please reload NT.", "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(-1);
                }
                finally
                {
                    WebClient.QueryString = new NameValueCollection();
                }
            }
            return null;
        }

        /// <summary>
        /// Posts data to the DB and handles WebExeptions
        /// </summary>
        /// <param name="uriPath">URI Path</param>
        /// <param name="postData">Data to upload</param>
        /// <returns>Response</returns>
        private string PostToDb(string uriPath, NameValueCollection postData)
        {
            Logger.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] is trying to achieve POST REST lock", GetType().Name);
            lock (RestOperationLock)
            {
                Logger.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] has achieved POST REST lock", GetType().Name);
                try
                {
                    var response = WebClient.UploadValues(new Uri(RemoteServerPath, uriPath), postData);
                    Logger.Log($"Thread [{Thread.CurrentThread.ManagedThreadId}] is releasing POST REST lock", GetType().Name);
                    return Encoding.Default.GetString(response);
                }
                catch (WebException e)
                {
                    return e.Message;
                }
            }
        }

        /// <summary>
        /// Adds a user to the database using server's RESTful API
        /// </summary>
        /// <param name="fullName">user's full name</param>
        /// <param name="userName">the requested username to add</param>
        /// <param name="password">the plain password</param>
        public void AddUser(string fullName, string userName, string password)
        {
            var postData = new NameValueCollection
            {
                { "fullname", fullName },
                { "username", userName },
                { "password", password },
            };

            PostToDb("user", postData);
        }

        /// <summary>
        /// Implement <see cref="IDisposable"/> interface to free unmanaged resources. i.e. open sockets.
        /// </summary>
        public void Dispose()
        {
            WebClient.Dispose();
        }

        public void SetObject(string key, dynamic value){}
    }
}
