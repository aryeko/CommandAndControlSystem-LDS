using System;
using System.Linq;
using System.Windows;
using ControlApplication.Core.Contracts;
using GMap.NET.WindowsPresentation;

namespace ControlApplication.DesktopClient.Controls.Markers
{
    public class MarkersVisitor : IMarkerableVisitor
    {
        private GMapControl mMapControl;

        public MarkersVisitor(GMapControl gMapControl)
        {
            mMapControl = gMapControl;
        }

        public void AddMarker(Detection detection)
        {
            var exsistingMarker = mMapControl.Markers.SingleOrDefault(m => m.Position.Equals(detection.Position));
            if (exsistingMarker != null)
            {
                DetectionMarker marker = exsistingMarker.Shape as DetectionMarker;
                marker?.AddDetection(detection);
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
            if (mMapControl.Markers.Any(m => m.Position.Equals(area.RootLocation)))
                return;//area already exists
            var marker = new GMapMarker(area.RootLocation);
            {
                var s = new AreaMarker(marker, area);
                marker.Shape = s;
            }
            mMapControl.Markers.Add(marker);
        }
    }
}
