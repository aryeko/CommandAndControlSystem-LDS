using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
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
        private readonly Uri RemoteServerPath = new Uri("http://localhost:5000");

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
            //try-catch
            var response = WebClient.UploadValues(new Uri(RemoteServerPath, "login") , postData);
            //webException
            return Encoding.Default.GetString(response).Contains("SUCCESS");
        }

        public List<Detection> GetDetections(string jsonFilter)
        {
            var postData = new NameValueCollection{{ "json_filter", jsonFilter }};
            //try-catch
            var response = WebClient.UploadValues(new Uri(RemoteServerPath, "login"), postData);
            //webException

            List<Detection> detectionsList = new List<Detection>();
            dynamic arr = JsonConvert.DeserializeObject(response.ToString());

            foreach (dynamic obj in arr)
            {
                DateTime dateTime = DateTime.ParseExact(obj.DateTimeOfDetection, "G", CultureInfo.CreateSpecificCulture("en-us"));
                PointLatLng position = new PointLatLng(Double.Parse(obj.Latitude) , Double.Parse(obj.Longitude));
                Material material = new Material(name, materialType);
                Detection detection = new Detection(dateTime, material, position, obj.SuspectId, obj.SuspectPlateId, obj.GunId);

                detectionsList.Add(detection);                
            }

            return detectionsList.Count.Equals(0) ? null : detectionsList;
        }

        public void AddDetection(Detection detection)
        {
            var postData = new NameValueCollection
            {
                
              //  { "material_name", detection.Material.Name },
              //  { "material_type", detection.Material.MaterialType.ToString() },
                //TODO: change to material_ID
                { "SuspectId", detection.SuspectId },
                { "SuspectPlateId", detection.SuspectPlateId },
                { "GunId", detection.GunId },
                { "Latitude", detection.Position.Lat.ToString()},
                { "Longitude", detection.Position.Lng.ToString()},
                { "DateTimeOfDetection", detection.DateTimeOfDetection.ToString("G", CultureInfo.CreateSpecificCulture("en-us")) }
                //TODO: post raman binary file
            };

            //try-catch
            WebClient.UploadValues(new Uri(RemoteServerPath, "detection"), postData);
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
