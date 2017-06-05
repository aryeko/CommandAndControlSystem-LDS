using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace ControlApplication.Core.Contracts
{
    public class Area : IMarkerable, IEquatable<Area>
    {
        public PointLatLng RootLocation { get; }

        public AreaType AreaType { get; }

        public double Radius { get; }

        public Area(PointLatLng rootLocation, AreaType areaType, double radius)
        {
            RootLocation = rootLocation;
            AreaType = areaType;
            Radius = radius;
        }

        public void Accept(IMarkerableVisitor visitor)
        {
            visitor.AddMarker(this);
        }

        public bool Equals(Area other)
        {
            if (other == null) return false;
            return RootLocation == other.RootLocation
                && AreaType.Equals(other.AreaType)
                && Radius.Equals(other.Radius);
        }

        public override int GetHashCode()
        {
            return RootLocation.GetHashCode() ^ AreaType.GetHashCode() ^ Radius.GetHashCode();
        }
    }
}
