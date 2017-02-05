using System;
using GMap.NET;

namespace ControlApplication.Core.Contracts
{
    /// <summary>
    /// This calss represents a detection
    /// TODO: Overload constructors for default values, i.e. auto date time
    /// </summary>
    public class Detection
    {
        /// <summary>
        /// The date of the detection
        /// </summary>
        public DateTime DateOfDetection { get; }

        /// <summary>
        /// The material detected
        /// </summary>
        public Material Material { get; }

        /// <summary>
        /// The position of detection
        /// </summary>
        public PointLatLng Position { get; }

        /// <summary>
        /// Suspect's ID number
        /// </summary>
        public string SuspectId { get; }

        /// <summary>
        /// Suspect's plate ID number
        /// </summary>
        public string SuspectPlateId { get; }

        /// <summary>
        /// G-Scan ID number
        /// </summary>
        public string GunId { get; }

        /// <summary>
        /// Detection class constructor
        /// </summary>
        /// <param name="dateTime">The date of the detection</param>
        /// <param name="material">The material detected</param>
        /// <param name="position">The position of detection</param>
        /// <param name="suspectId">Suspect's ID number</param>
        /// <param name="suspectPlateId">Suspect's plate ID number</param>
        /// <param name="gunId">G-Scan ID number</param>
        public Detection(DateTime dateTime, Material material, PointLatLng position, string suspectId,
            string suspectPlateId, string gunId)
        {
            DateOfDetection = dateTime;
            Material = material;
            Position = position;
            SuspectId = suspectId;
            SuspectPlateId = suspectPlateId;
            GunId = gunId;
        }
    }
}
