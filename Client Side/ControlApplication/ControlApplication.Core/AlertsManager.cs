using System;
using System.Collections.Generic;
using System.Globalization;
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

        public bool Handled { get; set; }

        public CombinationAlertArgs(string alertName, Area area, List<Detection> detections)
        {
            AlertName = alertName;
            Area = area;
            Detections = detections;
            Handled = false;
        }
    }

    public static class AlertsManager
    {
        public static event EventHandler<CombinationAlertArgs> CombinationFoundAlert;

        static AlertsManager()
        {
            Networking.Networking.GetNtServer().DetectionAdded += OnDetectionAdded;
        }

        private static void OnDetectionAdded(object sender, DetectionAddedEventArgs e)
        {
            var affectedAreaDetections = Networking.Networking.GetNtServer().GetDetections(areaId: e.Detection.Area.DatabaseId);
            var affectedAreaMaterials = affectedAreaDetections.Select(d => d.Material).ToList();

            var combinations = Networking.Networking.GetNtServer().GetMaterialsCombinationsAlerts();

            foreach (var combination in combinations)
            {
                if (combination.ContainesCombination(affectedAreaMaterials) 
                    && combination.CombinationMaterialsList.Contains(e.Detection.Material))
                {
                    var alertedDetections =
                        affectedAreaDetections.GroupBy(d => d.Material)
                            .Select(group => combination.CombinationMaterialsList.Contains(group.Key) ? group.Last() : null)
                                .Where(d => d != null)
                                .ToList();
                    AlertSystem(sender, combination.AlertName, e.Detection.Area, alertedDetections);
                }
            }
        }

        public static void AlertSystem(object source, string alertName, Area area, List<Detection> detections)
        {
            Logger.Log($"Alerting system for [{alertName}] alarm at area type [{area.AreaType}]");
            CombinationFoundAlert?.Invoke(source, new CombinationAlertArgs(alertName, area, detections));
        }
    }
}
