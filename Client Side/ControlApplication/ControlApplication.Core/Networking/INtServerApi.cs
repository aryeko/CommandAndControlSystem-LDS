using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;

namespace ControlApplication.Core.Networking
{
    public interface INtServerApi
    {
        /// <summary>
        /// Event useed to indicate that a detection has been added
        /// </summary>
        event EventHandler<DetectionAddedEventArgs> DetectionAdded;

        /// <summary>
        /// User login authentication 
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Returns whether the user is authorizes or not</returns>
        bool Login(string username, string password);

        /// <summary>
        /// Adds a user to the database using server's RESTful API
        /// </summary>
        /// <param name="fullName">user's full name</param>
        /// <param name="userName">the requested username to add</param>
        /// <param name="password">the plain password</param>
        void AddUser(string fullName, string userName, string password);

        /// <summary>
        /// gets materials from the database using server's RESTful API
        /// </summary>
        /// <param name="materialId">Optional: filter by material ID</param>
        /// <param name="name">Optional: filter by material name</param>
        /// <returns></returns>
        List<Material> GetMaterial(string materialId = "", string name = "");

        /// <summary>
        /// gets all areas or a specific area from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        List<Area> GetArea(string areaId = "");

        /// <summary>
        /// gets a G-Scan from the database using server's RESTful API
        /// </summary>
        /// <param name="gscanSn"></param>
        /// <param name="gscanId">Filter the search by G-Scan uniqe ID</param>
        /// <returns></returns>
        List<Gscan> GetGscan(string gscanSn = "", string gscanId = "");

        /// <summary>
        /// Adds a new Gscan to the database usint server's RESTfull API
        /// </summary>
        /// <param name="newGscan"></param>
        void AddGscan(Gscan newGscan);

        /// <summary>
        /// gets all detections from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        List<Detection> GetDetections(string areaId = "", string detectionId = "");

        /// <summary>
        /// gets all alerts from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
		List<Alert> GetAlerts();

        /// <summary>
        /// post an alert to the database using server's RESTful API
        /// </summary>
        /// <param name="alert"></param>
        void AddAlert(Alert alert);

        /// <summary>
        /// update an alert to the database by alert ID using server's RESTful API
        /// </summary>
        /// <param name="alert"></param>
        void UpdateAlert(Alert alert);

        /// <summary>
        /// gets a raman from the database using server's RESTful API
        /// </summary>
        /// <param name="ramanOutput"></param>
        /// <returns></returns>
        string GetRaman(string ramanOutput);

        /// <summary>
        /// Adds a detection to the database using server's RESTful API
        /// </summary>
        /// <param name="detection">A detection to add</param>
        /// <param name="idsDictionary">Dictionary of IDs</param>
        void AddDetection(Detection detection, Dictionary<string, string> idsDictionary = null);

        /// <summary>
        /// Adds a new area to the database using server's RESTful API
        /// </summary>
        /// <param name="newArea">An area to add</param>
        void AddArea(Area newArea);

        /// <summary>
        /// Adds a materials combination to the database using server's RESfull API
        /// </summary>
        /// <param name="combination">the combination to add</param>
        void AddMaterialsCombinationAlert(Combination combination);

        /// <summary>
        /// Get materials combination alert from the database using server's RESfull API
        /// </summary>
        /// <param name="combinationId">the combination ID to filter, gets all combinations if empty</param>
        /// <returns></returns>
        List<Combination> GetMaterialsCombinationsAlerts(string combinationId = "");

        /// <summary>
        /// A generic method for getting and setting objects to the memory cache.
        /// </summary>
        /// <param name="uriPath">URI Path</param>
        /// <param name="key">The key to get value from</param>
        /// <param name="value">The key value</param>
        /// <returns>An object of type dynamic</returns>
        dynamic GetObject(string uriPath, string key = "", string value = "");

        void SetObject(string key, dynamic value);
    }
}
