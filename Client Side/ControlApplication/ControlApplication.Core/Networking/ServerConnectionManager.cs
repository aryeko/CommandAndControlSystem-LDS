using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
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
        /// A single instance of <see cref="ServerConnectionManager"/> (Singelton)
        /// </summary>
        private static ServerConnectionManager _instance;

        /// <summary>
        /// Private constructor which will avoid from an external class to create another instance of <see cref="ServerConnectionManager"/>
        /// </summary>
        private ServerConnectionManager()
        {
            WebClient = new CookieAwareWebClient();
        }

        /// <summary>
        /// Creates an instance of <see cref="ServerConnectionManager"/> or returns the existing instance
        /// </summary>
        /// <returns>Returns single instance of <see cref="ServerConnectionManager"/> as long as the app is running</returns>
        public static ServerConnectionManager GetInstance()
        {
            return _instance ?? (_instance = new ServerConnectionManager());
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
                //GetAllDbIds(new Detection(DateTime.Now, new Material("Cocaine", MaterialType.Narcotics, ""),new PointLatLng(1122, 3344), "3027744552", "36-019-19", "1111"));
                return Encoding.Default.GetString(response).Contains("SUCCESS");
            }
            catch (WebException)
            {
                return false;
            }
        }

        public List<Material> GetMaterial()
        {
            var materials = new List<Material>();

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

            return materials;
        }

        public Material GetMaterial(string materialId)
        {
            WebClient.QueryString = new NameValueCollection
            {
                {"_id", materialId}
            };

            var response = WebClient.DownloadString(new Uri(RemoteServerPath, "material"));
            dynamic arr = JsonConvert.DeserializeObject(response);

            var materialType = (MaterialType)System.Enum.Parse(typeof(MaterialType), arr[0].type.ToString());
            return new Material(arr[0].name.ToString(), materialType, arr[0].cas.ToString());
        }

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

        //TODO: FINISH w/ server side
        public List<Detection> GetDetections(string jsonFilter)
        {
            var postData = new NameValueCollection { { "json_filter", jsonFilter } };
            //try-catch
            var response = WebClient.UploadValues(new Uri(RemoteServerPath, "detection"), postData);
            //webException

            var detectionsList = new List<Detection>();
            dynamic arr = JsonConvert.DeserializeObject(response.ToString());

            foreach (dynamic obj in arr[0])
            {
                DateTime dateTime = DateTime.ParseExact(obj.DateTimeOfDetection, "G", CultureInfo.CreateSpecificCulture("en-us"));
                var position = new PointLatLng(double.Parse(obj.Latitude), double.Parse(obj.Longitude));
                string gscanSn = GetGscan(obj.GunId.ToString());
                Area area = GetArea(obj.AreaId); //TODO: should we use Area?
                Material material = GetMaterial(obj.MaterialId);
                var detection = new Detection(dateTime, material, position, obj.SuspectId, obj.SuspectPlateId, gscanSn);
                
                detectionsList.Add(detection);
            }

            return detectionsList.Count.Equals(0) ? null : detectionsList;
        }

        //TODO: fix all post data values according to the server
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
                { "suspect_plate_id", detection.SuspectPlateId },
                { "Latitude", detection.Position.Lat.ToString(CultureInfo.InvariantCulture)},
                { "Longitude", detection.Position.Lng.ToString(CultureInfo.InvariantCulture)},
                { "DateTimeOfDetection", detection.DateTimeOfDetection.ToString("G", CultureInfo.CreateSpecificCulture("en-us")) }
                //TODO: post raman binary file
            };

            //try-catch
            WebClient.UploadValues(new Uri(RemoteServerPath, "detection"), postData);
        }

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
                {"root_location", $"[{detection.Position.Lat},{detection.Position.Lng}]"}
            };
            response = WebClient.DownloadString(new Uri(RemoteServerPath, "area"));
            arr = JsonConvert.DeserializeObject(response);
            ids.Add("AreaId", arr[0]._id.ToString());

            WebClient.QueryString = new NameValueCollection
            {
                {"gscan_sn", detection.GunId}
            };
            response = WebClient.DownloadString(new Uri(RemoteServerPath, "gscan"));
            arr = JsonConvert.DeserializeObject(response);
            ids.Add("GscanId", arr[0]._id.ToString());

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
        /// Adds a user to the database useing server's RESTful API
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
