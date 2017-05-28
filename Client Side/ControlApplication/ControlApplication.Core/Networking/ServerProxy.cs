using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;

namespace ControlApplication.Core.Networking
{
    /*
    public class ServerProxy : INtServerApi
    {

        private INtServerApi _realServerApi;

        public ServerProxy(INtServerApi realServerApi)
        {
            this._realServerApi = realServerApi;
        }

        public bool Login(string username, string password)
        {
            return _realServerApi.Login(username, password);
        }

        public List<Material> GetMaterial(string materialId = "", string name = "")
        {
            var materials = new List<Material>();
            dynamic response;
            if (!string.IsNullOrEmpty(name))
            {
                response = GetCachedObject(name);
            }
            else if (!string.IsNullOrEmpty(materialId))
            {
                response = GetCachedObject(materialId);
            }
            else
                return _realServerApi.GetMaterial();

            foreach (dynamic obj in response)
            {
                var materialType = (MaterialType)System.Enum.Parse(typeof(MaterialType), obj.type.ToString());
                materials.Add(new Material(obj.name.ToString(), materialType, obj.cas.ToString()));
            }

            return materials;
        }

        public List<Area> GetArea(string areaId = "")
        {
            if (!string.IsNullOrEmpty(areaId))
            {
                var area = GetCachedObject<Area>(areaId);
                if (area == null)
                {
                    area = _realServerApi.GetArea(areaId).FirstOrDefault();
                }
            }
        }

        public string GetGscan(string gscanId)
        {
            throw new NotImplementedException();
        }

        public List<Detection> GetDetections()
        {
            throw new NotImplementedException();
        }

        public string GetRaman(string ramanOutput)
        {
            throw new NotImplementedException();
        }

        public void AddDetection(Detection detection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A generic method for getting and setting objects to the memory cache.
        /// </summary>
        /// <param name="uriPath">URI Path</param>
        /// <param name="key">The key to get value from</param>
        /// <param name="value">The key value</param>
        /// <returns>An object of type dynamic</returns>
        private T GetCachedObject<T>(string value = "")
        {
            return CacheManager.GetObjectFromCache<T>(value);
        }

        /// <summary>
        /// A generic method for getting and setting objects to the memory cache.
        /// </summary>
        /// <param name="uriPath">URI Path</param>
        /// <param name="key">The key to get value from</param>
        /// <param name="value">The key value</param>
        /// <returns>An object of type dynamic</returns>
        private dynamic SetCachedObject(dynamic value, string key = "")
        {
            var response = CacheManager.GetObjectFromCache<dynamic>(value);
            if (response == null)
            {
                response = _realServerApi.GetFromDb(uriPath, key, value);
                CacheManager.SetObjectToCache(key, response);
            }
            return response;
        }
    }
    */
}
