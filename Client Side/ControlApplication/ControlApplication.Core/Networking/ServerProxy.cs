﻿using System;
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
            dynamic response = !string.IsNullOrEmpty(areaId)
                ? GetObject("area", "_id", areaId)
                : _realServerApi.GetObject("area");

            var areas = new List<Area>();

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

        public List<Alert> GetAlerts()
        {
            var alertsList = new List<Alert>();
            dynamic response = _realServerApi.GetObject("alert");

            foreach (var obj in response)
            {
                var detectionsIds = new List<string>();
                foreach (var detection in obj.detection_list)
                {
                    detectionsIds.Add(detection.ToString());
                }
                var detectionsList = detectionsIds.Select(detectionId => GetDetections(detectionId).First()).ToList();
                var area = GetArea(obj.area_id.ToString());
                alertsList.Add(ServerObjectConverter.ConvertAlert(obj, detectionsList, area));
            }
            return alertsList;
        }

        public List<Detection> GetDetections(string detectionId = "")
        {
            var detectionsList = new List<Detection>();
          
            dynamic response = !string.IsNullOrEmpty(detectionId) ?
                GetObject("detection", "_id", detectionId) :
                _realServerApi.GetObject("detection");

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
            ids.Add("MaterialId", response[0]._id.ToString());

            response = GetObject("area", "root_location", detection.Area.RootLocation.ToString());
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

        //private List<T> GetAllObjects<T>(string uriPath)
        //{
        //    var cachedObects = CacheManager.GetObjectFromCache<List<T>>($"all_objects_{typeof(T)}");
        //    if (cachedObects == null)
        //    {
        //        cachedObects = new List<T>();
        //        dynamic response = _realServerApi.GetObject(uriPath);
        //        foreach (var obj in response)
        //        {
        //            cachedObects.Add(ServerObjectConverter.Convert<T>(obj));
        //            CacheManager.SetObjectToCache(obj._id, obj);
        //        }
        //        CacheManager.SetObjectToCache($"all_objects_{typeof(T)}", cachedObects);
        //    }           

        //    return cachedObects;
        //}
    }
}
