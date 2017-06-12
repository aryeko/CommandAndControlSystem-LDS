using System;
using System.Collections.Generic;

namespace ControlApplication.Core.Contracts
{
    public class Alert
    {
        public string AlertName { get; }

        public Area Area { get; }

        public List<Detection> Detections { get; }

        public DateTime AlertTime { get; }

        public bool IsDirty { get; set; }

        public Alert(string alertName, Area area, List<Detection> detections, DateTime alertTime, bool isDirty = false)
        {
            AlertName = alertName;
            Area = area;
            Detections = detections;
            AlertTime = AlertTime;
            IsDirty = isDirty;
        }
    }
}