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
        }

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

            foreach (dynamic obj in response)
            {   
                materials.Add(ServerObjectConverter.ConvertMaterial(obj));
            }

            return materials;
        }

        public List<Area> GetArea(string areaId = "")
        {
            if (string.IsNullOrEmpty(areaId))
                return _realServerApi.GetArea();

            var areas = new List<Area>();
            dynamic response = GetObject("area", "_id", areaId);

            foreach (dynamic obj in response)
            {
                areas.Add(ServerObjectConverter.ConvertArea(obj));
            }

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

        public List<Detection> GetDetections()
        {
            var detectionsList = new List<Detection>();
            dynamic response = _realServerApi.GetObject("detection");

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
        /// Gets all the IDs from a detection in order to add a detection to DB using forein keys
        /// </summary>
        /// <param name="detection">The detection to add</param>
        /// <returns></returns>
        private Dictionary<string, string> GetAllDbIds(Detection detection)
        {
            var ids = new Dictionary<string, string>();

            dynamic response = GetObject("material", "material_name", detection.Material.Name);
            ids.Add("MaterialId", response[0]._id.ToString());

            response = GetObject("area", "root_location", $"[{detection.Area.RootLocation.Lat},{detection.Area.RootLocation.Lng}]");
            ids.Add("AreaId", response[0]._id.ToString());

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
                response = _realServerApi.GetObject(uriPath, key, value);
                CacheManager.SetObjectToCache(value, response);
            }
            return response;
        }
    }
}
