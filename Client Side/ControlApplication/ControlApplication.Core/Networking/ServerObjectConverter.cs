using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;
using GMap.NET;

namespace ControlApplication.Core.Networking
{
    internal static class ServerObjectConverter
    {
        /// <summary>
        /// Parse a location string from a JSON string
        /// </summary>
        /// <param name="location">The location </param>
        /// <returns>New PointLatLng using the parsed Lat and Lng</returns>
        public static PointLatLng ParseLocation(string location)
        {
            var pointPattern = @"{Lat=(?<lat>\d+\.*\d*?),\sLng=(?<lng>\d+\.*\d*?)}";

            var locationResult = Regex.Match(location, pointPattern);

            return new PointLatLng(double.Parse(locationResult.Groups["lat"].Value), double.Parse(locationResult.Groups["lng"].Value));
        }

        public static Area ConvertArea(dynamic obj)
        {
            var areaType = (AreaType)System.Enum.Parse(typeof(AreaType), obj.area_type.ToString());
            return new Area(ParseLocation(obj.root_location.ToString()), areaType, double.Parse(obj.radius.ToString()), obj._id.ToString());
        }

        public static Material ConvertMaterial(dynamic obj)
        {
            var materialType = (MaterialType)System.Enum.Parse(typeof(MaterialType), obj.type.ToString());
            return new Material(obj.name.ToString(), materialType, obj.cas.ToString(), obj._id.ToString());
        }

        public static string ConvertGscan(dynamic obj)
        {
            return obj[0].gscan_sn.ToString();
        }

        internal static Detection ConvertDetection(dynamic obj, Material material, Area area, string gscanSn, string ramanOutput)
        {
            var dateTime = DateTime.ParseExact(obj.date_time.ToString(), "G", CultureInfo.InvariantCulture);
            var position = ServerObjectConverter.ParseLocation(obj.location.ToString());
            return new Detection(dateTime, material, position, area, obj.suspect_id.ToString(), obj.plate_number.ToString(), gscanSn, ramanOutput, obj._id.ToString());
        }

        internal static Alert ConvertAlert(dynamic obj, List<Detection> detectionsList, Area area)
        {
            var isDirty = !obj.is_dirty.ToString().Equals("0");
            var dateTime = DateTime.ParseExact(obj.date_time.ToString(), "G", CultureInfo.InvariantCulture);
            return new Alert(obj.alert_name.ToString(), area, detectionsList, dateTime, isDirty);
        }

        public static Combination ConvertCombination(object o)
        {
            throw new NotImplementedException();
        }

        public static T Convert<T>(dynamic obj)
        {
            if (typeof(T) == typeof(Material))
                return ConvertMaterial(obj);

            throw new NotSupportedException($"Generic convert is not supported for type: {typeof(T)}");
        }

       
    }
}
