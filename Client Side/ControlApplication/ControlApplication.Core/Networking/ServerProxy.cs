using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;

namespace ControlApplication.Core.Networking
{
    internal class ServerProxy : INtServerApi
    {

        private readonly INtServerApi _realServerApi;

        public ServerProxy(INtServerApi realServerApi)
        {
            _realServerApi = realServerApi;
            _realServerApi.DetectionAdded += (sender, args) => DetectionAdded?.Invoke(sender, args);
        }

        public event EventHandler<DetectionAddedEventArgs> DetectionAdded;

        public bool Login(string username, string password)
        {
            return _realServerApi.Login(username, password);
        }

        public void AddUser(string fullName, string userName, string password)
        {
            _realServerApi.AddUser(fullName, userName, password);
        }

        public List<Material> GetMaterial(string materialId = "", string name = "")
        {
            var materials = new List<Material>();
            dynamic response;
            if (!string.IsNullOrEmpty(name))
            {
                response = GetObject("material", "material_name", name);
            }
            else if (!string.IsNullOrEmpty(materialId))
            {
                response = GetObject("material", "_id", materialId);
            }
            else
                return _realServerApi.GetMaterial();

            materials.Add(ServerObjectConverter.ConvertMaterial(response));
           
            return materials;
        }

        public List<Area> GetArea(string areaId = "")
        {
            if (string.IsNullOrEmpty(areaId))
                return _realServerApi.GetArea();

            dynamic response = GetObject("area", "_id", areaId);

            var areas = new List<Area> {ServerObjectConverter.ConvertArea(response)};

            return areas;
        }

        public List<string> GetGscan(string gscanId)
        {
            dynamic response;
            if (gscanId.Equals("no gscan"))
                return new List<string>() {""};

            if (!string.IsNullOrEmpty(gscanId))
                response = GetObject("gscan", "_id", gscanId);
            else
            {
                return _realServerApi.GetGscan();
            }

            return ServerObjectConverter.ConvertGscan(response);
        }

        public List<Alert> GetAlerts()
        {
            var alertsList = new List<Alert>();
            dynamic response = _realServerApi.GetObject("alert");

            foreach (dynamic obj in response)
            {
                var detectionsIds = new List<string>();
                foreach (dynamic detection in obj.detection_list)
                {
                    detectionsIds.Add(detection.ToString());
                }
                List<Detection> detectionsList = detectionsIds.Select(detectionId => GetDetections(detectionId: detectionId).First()).ToList();
                List<Area> area = GetArea(obj.area_id.ToString());
                alertsList.Add(ServerObjectConverter.ConvertAlert(obj, detectionsList, area.First()));
            }
            return alertsList;
        }

        public void AddAlert(Alert alert)
        {
            _realServerApi.AddAlert(alert);
        }

        public void UpdateAlert(Alert alert)
        {
            _realServerApi.UpdateAlert(alert);
        }

        public List<Detection> GetDetections(string areaId = "", string detectionId = "")
        {
            var detectionsList = new List<Detection>();
			dynamic response;
            if (!string.IsNullOrEmpty(areaId))
            {
                response = _realServerApi.GetObject("detection", "area_id", areaId);
            }
			else if(!string.IsNullOrEmpty(detectionId))
            {
                response = GetObject("detection", "_id", detectionId);
            }
            else
            {
                response = _realServerApi.GetObject("detection");
            }
            
            foreach (dynamic obj in response)
            {
                var gscanSn = GetGscan(obj.gscan_id.ToString());
                var ramanOutput = GetRaman(obj.raman_output_id.ToString());
                var area = GetArea(obj.area_id.ToString());
                var material = GetMaterial(materialId: obj.material_id.ToString());
                detectionsList.Add(ServerObjectConverter.ConvertDetection(obj, material[0], area[0], gscanSn[0], ramanOutput));
            }

            return detectionsList;
        }

        public string GetRaman(string ramanOutput)
        {
            if (ramanOutput.Equals("no raman"))
                return "";
            return "";
        }

        public void AddDetection(Detection detection, Dictionary<string, string> idsDictionary = null)
        {
            if(idsDictionary == null)
                idsDictionary = GetAllDbIds(detection);
            _realServerApi.AddDetection(detection, idsDictionary);
        }

        public void AddArea(Area newArea)
        {
            _realServerApi.AddArea(newArea);
        }

        /// <summary>
        /// Adds a materials combination to the database using server's RESfull API
        /// </summary>
        /// <param name="combination">the combination to add</param>
        public void AddMaterialsCombinationAlert(Combination combination)
        {
            _realServerApi.AddMaterialsCombinationAlert(combination);
        }

        /// <summary>
        /// Get materials combination alert from the database using server's RESfull API
        /// </summary>
        /// <param name="combinationId">the combination ID to filter, gets all combinations if empty</param>
        /// <returns></returns>
        public List<Combination> GetMaterialsCombinationsAlerts(string combinationId = "")
        {
            var combinationsList = new List<Combination>();
            dynamic response = !string.IsNullOrEmpty(combinationId) ?
                GetObject("materials_combination", "_id", combinationId) :
                _realServerApi.GetObject("materials_combination");

            foreach (dynamic obj in response)
            {
                //TODO: move to ServerObjectConverter?
                var materialsIds = new List<string>();
                foreach (dynamic o in obj.materials_list)
                {
                    materialsIds.Add(o.ToString());
                }
                var materialsList = materialsIds.Select(materialId => GetMaterial(materialId: materialId).First()).ToList();
                combinationsList.Add(new Combination(obj.alert_name.ToString(), materialsList));
            }

            return combinationsList;
        }

        /// <summary>
        /// Gets all the IDs from a detection in order to add a detection to DB using forein keys
        /// </summary>
        /// <param name="detection">The detection to add</param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllDbIds(Detection detection)
        {
            var ids = new Dictionary<string, string>();

            dynamic response = GetObject("material", "material_name", detection.Material.Name);
            ids.Add("MaterialId", response._id.ToString());

            response = GetObject("area", "root_location", detection.Area.RootLocation.ToString());
            ids.Add("AreaId", response._id.ToString());

            if (string.IsNullOrEmpty(detection.GunId))
                ids.Add("GscanId", "");
            else
            {
                response = GetObject("gscan", "gscan_sn", detection.GunId);
                ids.Add("GscanId", response[0]._id.ToString());
            }

            response = GetObject("user", "username", "lds");
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
        public dynamic GetObject(string uriPath, string key = "", string value = "")
        {
            var response = CacheManager.GetObjectFromCache<dynamic>(value);
            if (response == null)
            {
                Logger.Log($"{value} *NOT* in cache");
                response = _realServerApi.GetObject(uriPath, key, value);
                SetObject(value, response);
            }
            else
                Logger.Log($"{value} *IN* cache");

            return response;
        }

        public void SetObject(string key, dynamic value)
        {
            CacheManager.SetObjectToCache(key, value);
        }
    }
}
