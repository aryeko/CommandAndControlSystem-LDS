﻿using System;
using GMap.NET;

namespace ControlApplication.Core.Contracts
{
    /// <summary>
    /// This calss represents a detection
    /// TODO: Overload constructors for default values, i.e. auto date time
    /// </summary>
    public class Detection : IMarkerable, IEquatable<Detection>
    {
        /// <summary>
        /// The date of the detection
        /// </summary>
        public DateTime DateTimeOfDetection { get; }

        /// <summary>
        /// The material detected
        /// </summary>
        public Material Material { get; }

        /// <summary>
        /// The position of detection
        /// </summary>
        public PointLatLng Position { get; }

        /// <summary>
        /// The area of the detection
        /// </summary>
        public Area Area { get; }

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
        /// Raman graph ID
        /// </summary>
        public string RamanGraph { get; }

        /// <summary>
        /// The Database ID
        /// </summary>
        public string DatabaseId { get; }

        /// <summary>
        /// Detection class constructor
        /// </summary>
        /// <param name="dateTime">The date of the detection</param>
        /// <param name="material">The material detected</param>
        /// <param name="position">The position of detection</param>
        /// <param name="area">The area of the detection</param>
        /// <param name="suspectId">Suspect's ID number</param>
        /// <param name="suspectPlateId">Suspect's plate ID number</param>
        /// <param name="gunId">G-Scan ID number</param>
        /// <param name="ramanGraph">Raman graph ID</param>
        /// <param name="databaseId">DB ID</param>
        public Detection(DateTime dateTime, Material material, PointLatLng position, Area area, string suspectId,
            string suspectPlateId, string gunId = "", string ramanGraph = "", string databaseId = "")
        {
            DateTimeOfDetection = dateTime;
            Material = material;
            Position = position;
            Area = area;
            SuspectId = suspectId;
            SuspectPlateId = suspectPlateId;
            GunId = gunId;
            RamanGraph = ramanGraph;
            DatabaseId = databaseId;
        }

        public void Accept(IMarkerableVisitor visitor)
        {
            visitor.AddMarker(this);
        }

        public bool Equals(Detection other)
        {
            if (other == null) return false;
            return DateTimeOfDetection.ToString("G").Equals(other.DateTimeOfDetection.ToString("G"))
                    && Material.Equals(other.Material);
        }

        public override int GetHashCode()
        { 
            return Material.GetHashCode() ^ DateTimeOfDetection.ToString("G").GetHashCode();
        }
    }
}
