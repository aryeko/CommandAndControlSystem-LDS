using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;

namespace ControlApplication.Core
{
    public class CombinationAlertArgs : EventArgs
    {
        public string AlertName { get; }

        public Area Area { get; }

        public List<Detection> Detections { get; }

        public DateTime AlertTime { get; } 

        public CombinationAlertArgs(string alertName, Area area, List<Detection> detections)
        {
            AlertName = alertName;
            Area = area;
            Detections = detections;
            AlertTime = DateTime.Now;
        }
    }

    public static class AlertsManager
    {
        public delegate void CombinationFoundAlertHandler(object source, CombinationAlertArgs args);

        public static event CombinationFoundAlertHandler CombinationFoundAlert;

        public static void AlertSystem(object source, string alertName, Area area, List<Detection> detections)
        {
            CombinationFoundAlert?.Invoke(source, new CombinationAlertArgs(alertName, area, detections));
        }
    }
}
