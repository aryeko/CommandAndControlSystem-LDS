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
using GMap.NET;
using Newtonsoft.Json;

namespace ControlApplication.Core.Networking
{
    /// <summary>
    /// A class that manages the Server connection and creates a single instance of <see cref="ServerConnectionManager"/> (Singelton)
    /// The class provides a detailed API for sessioned communication with the Server.
    /// </summary>
    public class ServerConnectionManager : IDisposable
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
            string response;
            if (!string.IsNullOrEmpty(name))
            {
                response = GetFromDb("material", "material_name", name);
            }
            else if(!string.IsNullOrEmpty(materialId))
            {
                response = GetFromDb("material", "_id", materialId);
            }
            else
                response = GetFromDb("material");

            dynamic arr = JsonConvert.DeserializeObject(response);

            foreach (dynamic obj in arr)
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
            var response = !string.IsNullOrEmpty(areaId) ? GetFromDb("area", "_id", areaId) : GetFromDb("area");

            dynamic arr = JsonConvert.DeserializeObject(response);

            foreach (dynamic obj in arr)
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
            var response = GetFromDb("gscan", "_id", gscanId);
            dynamic arr = JsonConvert.DeserializeObject(response);

            return arr[0].gscan_sn.ToString();
        }

        /// <summary>
        /// gets all detections from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        public List<Detection> GetDetections()
        {
            var detectionsList = new List<Detection>();
            var response = GetFromDb("detection");

            dynamic arr = JsonConvert.DeserializeObject(response);
            
            foreach (dynamic obj in arr)
            {
                DateTime dateTime = DateTime.ParseExact(obj.date_time.ToString(), "G", CultureInfo.InvariantCulture);
                var position = ParseLocation(obj.location.ToString());
                string gscanSn = GetGscan(obj.gscan_id.ToString());
                var area = GetArea(obj.area_id.ToString());
                var material = GetMaterial(materialId:obj.material_id.ToString());
                var detection = new Detection(dateTime, material[0], position, area[0], obj.suspect_id.ToString(), obj.plate_number.ToString(), gscanSn, obj.raman_output_id.ToString());
                
                detectionsList.Add(detection);
            }

            return detectionsList;
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

            var response = GetFromDb("material", "material_name", detection.Material.Name);
            dynamic arr = JsonConvert.DeserializeObject(response);
            ids.Add("MaterialId", arr[0]._id.ToString());

            response = GetFromDb("area", "root_location", $"[{detection.Area.RootLocation.Lat},{detection.Area.RootLocation.Lng}]");
            arr = JsonConvert.DeserializeObject(response);
            ids.Add("AreaId", arr[0]._id.ToString());

            response = GetFromDb("gscan", "gscan_sn", detection.GunId);
            if (response.Equals("[]"))
                ids.Add("GscanId", "");
            else
            {
                arr = JsonConvert.DeserializeObject(response);
                ids.Add("GscanId", arr[0]._id.ToString());
            }
            
            response = GetFromDb("user", "username", "lds");
            arr = JsonConvert.DeserializeObject(response);
            ids.Add("UserId", arr[0]._id.ToString());

            return ids;
        }

        /// <summary>
        /// Gets data from the DB and handles WebExeptions
        /// </summary>
        /// <param name="uriPath">URI Path</param>
        /// <param name="key">The key to get value from</param>
        /// <param name="value">The key value</param>
        /// <returns>Response from DB</returns>
        public string GetFromDb(string uriPath, string key = "", string value = "")
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
                return WebClient.DownloadString(new Uri(RemoteServerPath, uriPath));
            }
            catch (WebException e)
            {
                //MessageBox.Show("Your session expired! please reload NT.", "Authentication failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Exception caught: " + e.Message);
                //Environment.Exit(-1);
            }
            finally
            {
                WebClient.QueryString = new NameValueCollection();
            }

            return "";
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
