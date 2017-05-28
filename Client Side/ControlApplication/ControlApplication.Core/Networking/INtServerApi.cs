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
        /// User login authentication 
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Returns whether the user is authorizes or not</returns>
        bool Login(string username, string password);

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
        /// <param name="gscanId">Filter the search by G-Scan uniqe ID</param>
        /// <returns></returns>
        string GetGscan(string gscanId);

        /// <summary>
        /// gets all detections from the database using server's RESTful API
        /// </summary>
        /// <returns></returns>
        List<Detection> GetDetections();

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
        void AddDetection(Detection detection);



    }
}
