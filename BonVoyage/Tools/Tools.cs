using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    static class Tools
    {
        /// <returns>
		/// The full relative path from the main KSP folder to a given texture resource from this mod.
		/// </returns>
		/// <param name="filename">Name of file located in our plugin folder</param>
		/// <param name="GameDataRelative">True if the KSP/GameData portion of the path is assumed, false if we need to provide the full path</param>
		public static string TextureFilePath(string filename, bool GameDataRelative = true)
        {
            if (GameDataRelative)
            {
                return string.Format("{0}/Textures/{1}", BonVoyage.Name, filename);
            }
            else
            {
                return string.Format("{0}/Textures/{1}",
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    filename);
            }
        }


        /// <summary>
        /// Convert distance in meters to text (meters or kilometers)
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static string ConvertDistanceToText(double distance)
        {
            string result = "-";
            double n = distance;
            if (n > 0)
            {
                if (n < 1000)
                    result = n.ToString("N0") + " m";
                else
                {
                    n = n / 1000;
                    result = n.ToString("0.##") + " km";
                }
            }
            return result;
        }

    }

}
