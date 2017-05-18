using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

            try
            {
                var response = WebClient.UploadValues(new Uri(RemoteServerPath, "login"), postData);
                return Encoding.Default.GetString(response).Contains("SUCCESS");
            }
            catch (WebException)
            {
                return false;
            }
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
            if (!string.IsNullOrEmpty(name))
            {
                WebClient.QueryString = new NameValueCollection
                {
                    {"material_name", name}
                };
            }
            else if(!string.IsNullOrEmpty(materialId))
            {
                WebClient.QueryString = new NameValueCollection
                {
                    {"_id", materialId}
                };
            }
            else
                WebClient.QueryString = new NameValueCollection();
            
            try
            {
                var response = WebClient.DownloadString(new Uri(RemoteServerPath, "material"));
                dynamic arr = JsonConvert.DeserializeObject(response);

                foreach (dynamic obj in arr)
                {
                    var materialType = (MaterialType) System.Enum.Parse(typeof(MaterialType), obj.type.ToString());
                    materials.Add(new Material(obj.name.ToString(), materialType, obj.cas.ToString()));
                }
            }
            catch (WebException)
            {
                return null;
            }
            finally
            {
                WebClient.QueryString = new NameValueCollection();
            }

            return materials;
        }

        /// <summary>
        /// gets all areas from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        public List<Area> GetArea()
        {
            var areas = new List<Area>();

            try
            {
                var response = WebClient.DownloadString(new Uri(RemoteServerPath, "area"));
                dynamic arr = JsonConvert.DeserializeObject(response);

                foreach (dynamic obj in arr)
                {
                    var areaType = (AreaType)System.Enum.Parse(typeof(AreaType), obj.area_type.ToString());
                    areas.Add(new Area(new PointLatLng(double.Parse(obj.root_location[0].ToString()), double.Parse(obj.root_location[1].ToString())), areaType, double.Parse(obj.radius.ToString())));
                }
            }
            catch (WebException)
            {
                return null;
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
            WebClient.QueryString = new NameValueCollection
            {
                {"_id", gscanId}
            };

            var response = WebClient.DownloadString(new Uri(RemoteServerPath, "gscan"));
            dynamic arr = JsonConvert.DeserializeObject(response);

            return arr[0].gscan_sn.ToString();
        }

        /// <summary>
        /// gets an Area from the database using server's RESTful API
        /// </summary>
        /// <param name="areaId">Filter the search by area uniqe ID</param>
        /// <returns></returns>
        public Area GetArea(string areaId)
        { 
        
         WebClient.QueryString = new NameValueCollection
            {
                {"_id", areaId}
            };

            var response = WebClient.DownloadString(new Uri(RemoteServerPath, "area"));
            dynamic arr = JsonConvert.DeserializeObject(response);

            var areaType = (AreaType)System.Enum.Parse(typeof(AreaType), arr[0].area_type.ToString());
            return new Area(new PointLatLng(double.Parse(arr[0].root_location[0].ToString()), double.Parse(arr[0].root_location[1].ToString())), areaType, double.Parse(arr[0].radius.ToString()));
        }

        /// <summary>
        /// gets all detections from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        public List<Detection> GetDetections()
        {
            var detectionsList = new List<Detection>();
            var response = WebClient.DownloadString(new Uri(RemoteServerPath, "detection"));
            //webException

            dynamic arr = JsonConvert.DeserializeObject(response);
            
            foreach (dynamic obj in arr)
            {
                DateTime dateTime = DateTime.ParseExact(obj.date_time.ToString(), "G", CultureInfo.InvariantCulture);
                var position = ParseLocation(obj.location.ToString()); //new PointLatLng(double.Parse(obj.Latitude), double.Parse(obj.Longitude));
                string gscanSn = GetGscan(obj.gscan_id.ToString());
                Area area = GetArea(obj.area_id.ToString());
                var material = GetMaterial(materialId:obj.material_id.ToString());
                var detection = new Detection(dateTime, material[0], position, area, obj.suspect_id.ToString(), obj.plate_number.ToString(), gscanSn, obj.raman_output_id.ToString());
                
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

            try
            {
                WebClient.UploadValues(new Uri(RemoteServerPath, "detection"), postData);
            }
            catch (WebException)
            {
                Console.WriteLine("Exception caught");
            }
        }

        /// <summary>
        /// Gets all the IDs from a detection in order to add a detection to DB using forein keys
        /// </summary>
        /// <param name="detection">The detection to add</param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllDbIds(Detection detection)
        {
            var ids = new Dictionary<string, string>();

            WebClient.QueryString = new NameValueCollection
            {
                {"material_name", detection.Material.Name}
            };
            var response = WebClient.DownloadString(new Uri(RemoteServerPath, "material"));
            dynamic arr = JsonConvert.DeserializeObject(response);
            ids.Add("MaterialId", arr[0]._id.ToString());

            WebClient.QueryString = new NameValueCollection
            {
                {"root_location", $"[{detection.Area.RootLocation.Lat},{detection.Area.RootLocation.Lng}]"}
            };
            response = WebClient.DownloadString(new Uri(RemoteServerPath, "area"));
            arr = JsonConvert.DeserializeObject(response);
            ids.Add("AreaId", arr[0]._id.ToString());

            WebClient.QueryString = new NameValueCollection
            {
                {"gscan_sn", detection.GunId}
            };
            response = WebClient.DownloadString(new Uri(RemoteServerPath, "gscan"));
            if (response.Equals("[]"))
                ids.Add("GscanId", "");
            else
            {
                arr = JsonConvert.DeserializeObject(response);
                ids.Add("GscanId", arr[0]._id.ToString());
            }

            WebClient.QueryString = new NameValueCollection
            {
                {"username", "lds"}
            };
            response = WebClient.DownloadString(new Uri(RemoteServerPath, "user"));
            arr = JsonConvert.DeserializeObject(response);
            ids.Add("UserId", arr[0]._id.ToString());

            return ids;
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

            //try-catch
            WebClient.UploadValues(new Uri(RemoteServerPath, "user"), postData);
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
