using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ControlApplication.Core.Contracts;
using ControlApplication.Core.Networking;
using System.Runtime.Caching;
using System.IO;
using GMap.NET;
using Newtonsoft.Json;

namespace ControlApplication.Core.Networking
{
    /// <summary>
    /// A class that manages the Server connection and creates a single instance of <see cref="ServerConnectionManager"/> (Singelton)
    /// The class provides a detailed API for sessioned communication with the Server.
    /// </summary>
    public class ServerConnectionManager : INtServerApi, IDisposable
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
        /// Creates an instance of <see cref="ServerConnectionManager"/> or returns the existing instance
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
            dynamic response;
            if (!string.IsNullOrEmpty(name))
            {
                response = GetCachedObject("material", "material_name", name);
            }
            else if(!string.IsNullOrEmpty(materialId))
            {
                response = GetCachedObject("material", "_id", materialId);
            }
            else
                response = GetFromDb("material");

            foreach (dynamic obj in response)
            {
                var materialType = (MaterialType) System.Enum.Parse(typeof(MaterialType), obj.type.ToString());
                materials.Add(new Material(obj.name.ToString(), materialType, obj.cas.ToString()));
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
            dynamic response = !string.IsNullOrEmpty(areaId) ? GetCachedObject("area", "_id", areaId) : GetFromDb("area");

            foreach (dynamic obj in response)
            {
                var areaType = (AreaType)System.Enum.Parse(typeof(AreaType), obj.area_type.ToString());
                areas.Add(new Area(ParseLocation(obj.root_location.ToString()), areaType, double.Parse(obj.radius.ToString())));
            }

            return areas;
        }

        /// <summary>
        /// gets a G-Scan from the database using server's RESTful API
        /// </summary>
        /// <param name="gscanId">Filter the search by G-Scan uniqe ID</param>
        /// <returns></returns>
        public string GetGscan(string gscanId)
        {
            if (gscanId.Equals("no gscan"))
                return "";

            dynamic response = GetCachedObject("gscan", "_id", gscanId);

            return response[0].gscan_sn.ToString();
        }

        /// <summary>
        /// gets all detections from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        public List<Detection> GetDetections()
        {
            var detectionsList = new List<Detection>();
            dynamic response = GetFromDb("detection");

            foreach (dynamic obj in response)
            {
                DateTime dateTime = DateTime.ParseExact(obj.date_time.ToString(), "G", CultureInfo.InvariantCulture);
                var position = ParseLocation(obj.location.ToString());
                string gscanSn = GetGscan(obj.gscan_id.ToString());
                string ramanOutput = GetRaman(obj.raman_output_id.ToString());
                var area = GetArea(obj.area_id.ToString());
                var material = GetMaterial(materialId:obj.material_id.ToString());
                var detection = new Detection(dateTime, material[0], position, area[0], obj.suspect_id.ToString(), obj.plate_number.ToString(), gscanSn, ramanOutput);
                
                detectionsList.Add(detection);
            }

            return detectionsList;
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
            //dynamic response = GetCachedObject("raman", "_id", ramanOutput);
            //return response[0]._id.ToString();
            return "";  
        }

        /// <summary>
        /// Parse a location string from a JSON string
        /// </summary>
        /// <param name="location">The location </param>
        /// <returns>New PointLatLng using the parsed Lat and Lng</returns>
        private PointLatLng ParseLocation(string location)
        {
            var pointPattern = @"{Lat=(?<lat>\d+\.*\d*?),\sLng=(?<lng>\d+\.*\d*?)}";

            var locationResult = Regex.Match(location, pointPattern);
            
            return new PointLatLng(double.Parse(locationResult.Groups["lat"].Value), double.Parse(locationResult.Groups["lng"].Value));
        }

        /// <summary>
        /// Adds a detection to the database using server's RESTful API
        /// </summary>
        /// <param name="detection">A detection to add</param>
        public void AddDetection(Detection detection)
        {
            var ids = GetAllDbIds(detection);
            var postData = new NameValueCollection
            {
                { "user_id", ids["UserId"] },
                { "material_id", ids["MaterialId"] },
                { "area_id", ids["AreaId"] },
                { "gscan_id", ids["GscanId"] },               
                { "suspect_id", detection.SuspectId },
                { "raman_id", detection.RamanId },
                { "plate_number", detection.SuspectPlateId },
                { "location", detection.Position.ToString() },
                { "date_time", detection.DateTimeOfDetection.ToString("G", CultureInfo.InvariantCulture) }
            };

            PostToDb("detection", postData);
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
                { "radius", newArea.Radius.ToString() },
            };

            PostToDb("area", postData);
        }

        /// <summary>
        /// Gets all the IDs from a detection in order to add a detection to DB using forein keys
        /// </summary>
        /// <param name="detection">The detection to add</param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllDbIds(Detection detection)
        {
            var ids = new Dictionary<string, string>();

            dynamic response = GetCachedObject("material", "material_name", detection.Material.Name);
            ids.Add("MaterialId", response[0]._id.ToString());

            response = GetCachedObject("area", "root_location", $"[{detection.Area.RootLocation.Lat},{detection.Area.RootLocation.Lng}]");
            ids.Add("AreaId", response[0]._id.ToString());

            if (string.IsNullOrEmpty(detection.GunId))
                ids.Add("GscanId", "");
            else
            {
                response = GetCachedObject("gscan", "gscan_sn", detection.GunId);
                ids.Add("GscanId", response[0]._id.ToString());
            }
            
            response = GetCachedObject("user", "username", "lds");
            ids.Add("UserId", response[0]._id.ToString());

            return ids;
        }

        /// <summary>
        /// A generic method for getting and setting objects to the memory cache.
        /// </summary>
        /// <param name="uriPath">URI Path</param>
        /// <param name="key">The key to get value from</param>
        /// <param name="value">The key value</param>
        /// <returns>An object of type dynamic</returns>
        private dynamic GetCachedObject(string uriPath, string key = "", string value = "")
        {
            var response = CacheManager.GetObjectFromCache<dynamic>(value);
            if (response == null)
            {
                response = GetFromDb(uriPath, key, value);
                CacheManager.SetObjectToCache(value, response);
            }
            return response;
        }

        /// <summary>
        /// Gets data from the DB and handles WebExeptions
        /// </summary>
        /// <param name="uriPath">URI Path</param>
        /// <param name="key">The key to get value from</param>
        /// <param name="value">The key value</param>
        /// <returns>Response from DB</returns>
        private dynamic GetFromDb(string uriPath, string key = "", string value = "")
        {
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
            try
            {
                var response = WebClient.UploadValues(new Uri(RemoteServerPath, uriPath), postData);
                return Encoding.Default.GetString(response);
            }
            catch (WebException e)
            {             
                return e.Message;                
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
    }
}
