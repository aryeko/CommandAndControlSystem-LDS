using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace ControlApplication.Core.Networking
{
    public static class CacheManager
    {
        public static ObjectCache Cache { get; } = MemoryCache.Default;

        /// <summary>
        /// How long to cache an object for.
        /// </summary>
        public const double CacheTimeHours = 24; //1 day

        /// <summary>
        /// A generic method for getting objects to the memory cache.
        /// </summary>
        /// <typeparam name="T">The type of the object to be returned.</typeparam>
        /// <param name="cacheItemName">The name to be used when storing this object in the cache.</param>
        /// <returns>An object of type T</returns>
        public static T GetObjectFromCache<T>(string cacheItemName)
        {
            return (T)Cache[cacheItemName];
        }

        /// <summary>
        /// A generic method for setting objects to the memory cache.
        /// </summary>
        /// <param name="cacheItemName">The name to be used when storing this object in the cache.</param>
        /// <param name="objectToCache">A parameterless function to call if the object isn't in the cache and you need to set it.</param>
        public static void SetObjectToCache(string cacheItemName, dynamic objectToCache)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(CacheTimeHours)
            };
            Cache.Set(cacheItemName, objectToCache, policy);
        }
    }
}
