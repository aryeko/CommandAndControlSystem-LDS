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

        public bool Handled { get; set; }

        public CombinationAlertArgs(string alertName, Area area, List<Detection> detections)
        {
            AlertName = alertName;
            Area = area;
            Detections = detections;
            AlertTime = DateTime.Now;
            Handled = false;
        }
    }

    public static class AlertsManager
    {
        public static event EventHandler<CombinationAlertArgs> CombinationFoundAlert;

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
                if (combination.ContainesCombination(affectedAreaMaterials) 
                    && combination.CombinationMaterialsList.Contains(e.Detection.Material))
                {
                    var alertedDetections =
                        affectedAreaDetections.GroupBy(d => d.Material)
                            .Select(
                                group => combination.CombinationMaterialsList.Contains(group.Key) ? group.Last() : null).ToList();
                    AlertSystem(sender, combination.AlertName, e.Detection.Area, alertedDetections);
                }
            }
        }

        public static void AlertSystem(object source, string alertName, Area area, List<Detection> detections)
        {
            CombinationFoundAlert?.Invoke(source, new CombinationAlertArgs(alertName, area, detections));
        }
    }
}
