using System;
using System.Collections.Generic;

namespace BonVoyage
{
    /// <summary>
    /// Pathfinder helper utils
    /// </summary>
    public class PathUtils
    {
        /// <summary>
        /// Path waypoint
        /// </summary>
        internal struct WayPoint
        {
            public double latitude;
            public double longitude;
            public WayPoint(double lat, double lon)
            {
                latitude = lat;
                longitude = lon;
            }
        }


        /// <summary>
        /// Encode path to base64 string
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string EncodePath(Path<Hex> path)
        {
            string result = "";
            foreach (Hex point in path)
            {
                result += point.Latitude.ToString("R") + ":" + point.Longitude.ToString("R") + ";";
            }
            
            // Replace forward slash with # (two forward slashes seems to be interpreted as a start of the comment when read from a save file)
            var textBytes = System.Text.Encoding.UTF8.GetBytes(result);
            return Convert.ToBase64String(textBytes).Replace('/', '#');
        }


        /// <summary>
        /// Decode path from base64 encoded string
        /// </summary>
        /// <param name="pathEncoded"></param>
        /// <returns></returns>
        internal static List<WayPoint> DecodePath(string pathEncoded)
        {
            if (pathEncoded == null || pathEncoded.Length == 0)
                return null;

            // Path is compressed, decompress
            // Replace # with forward slash (two forward slashes seems to be interpreted as a start of the comment when read from a save file)
            var encodedBytes = Convert.FromBase64String(pathEncoded.Replace('#', '/'));
            pathEncoded = System.Text.Encoding.UTF8.GetString(encodedBytes);

            List<WayPoint> result = new List<WayPoint>();
            char[] separators = new char[] { ';' };
            string[] wps = pathEncoded.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < wps.Length; i++)
            {
                string[] latlon = wps[i].Split(':');
                result.Add(new WayPoint(double.Parse(latlon[0]), double.Parse(latlon[1])));
            }
            result.Reverse();
            return result;
        }

    }

}
