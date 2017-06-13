using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;
using ControlApplication.Core.Networking;

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

        static AlertsManager()
        {
            NetworkClientsFactory.GetNtServer().DetectionAdded += OnDetectionAdded;
        }

        private static void OnDetectionAdded(object sender, DetectionAddedEventArgs e)
        {
            var affectedAreaDetections = NetworkClientsFactory.GetNtServer().GetDetections(e.Detection.Area.DatabaseId);
            var affectedAreaMaterials = affectedAreaDetections.Select(d => d.Material).ToList();

            var combinations = NetworkClientsFactory.GetNtServer().GetMaterialsCombinationsAlerts();

            foreach (var combination in combinations)
            {
                if (combination.ContainesCombination(affectedAreaMaterials))
                {
                    var alertedDetections =
                        affectedAreaDetections.GroupBy(d => d.Material)
                            .Select(
                                group => combination.CombinationMaterialsList.Contains(group.Key) ? group.Last() : null).ToList();
                    AlertSystem(sender, e.Detection.Area, alertedDetections);
                }
            }
        }

        public static void AlertSystem(object source, string alertName, Area area, List<Detection> detections)
        {
            CombinationFoundAlert?.Invoke(source, new CombinationAlertArgs(alertName, area, detections));
        }
    }
}
