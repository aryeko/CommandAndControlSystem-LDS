using System;
using System.Linq;
using System.Windows;
using ControlApplication.Core.Contracts;
using GMap.NET.WindowsPresentation;

namespace ControlApplication.DesktopClient.Controls.Markers
{
    public class MarkersManager : IMarkerableVisitor
    {
        private GMapControl mMapControl;

        public MarkersManager(GMapControl gMapControl)
        {
            mMapControl = gMapControl;
        }

        public void AddMarker(Detection detection)
        {
            var exsistingMarker = mMapControl.Markers.SingleOrDefault(m => m.Position.Equals(detection.Position));
            if (exsistingMarker != null)
            {
                ((DetectionMarker)exsistingMarker.Shape).AddDetection(detection);
            }
            else
            {
                var marker = new GMapMarker(detection.Position);
                {
                    var s = new DetectionMarker(marker, detection);
                    marker.Shape = s;
                }
                mMapControl.Markers.Add(marker);
            }
        }

        public void AddMarker(Area area)
        {
            throw new NotImplementedException();
        }
    }
}
