using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;

namespace ControlApplication.Core.Contracts
{
    public class Area
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
    }
}
