using System.IO;
using System.Linq;
using System.Reflection;

namespace AnodyneSharp.Map
{
    public static class MapLoader
    {
        public static string[] maps_with_fg = new string[] { "DEBUG", "SPACE", "STREET", "APARTMENT", "HOTEL", "NEXUS", "CIRCUS", "FOREST", "REDSEA", "FIELDS" };
        public static string[] maps_with_bg2 = new string[] { "TERMINAL", "SPACE", "DEBUG", "OVERWORLD", "HAPPY", "BLUE", "REDCAVE", "STREET", "APARTMENT", "HOTEL", "CROWD", "WINDMILL", "CLIFF", "FOREST", "GO" };

        private static readonly string[] map_suffix = new string[] { "BG", "BG2", "FG" };

        public static string GetMap(string mapName, int layer = 1)
        {
            string CSV = "0,0";

            if (layer == 3 && !maps_with_fg.Contains(mapName))
            {
                return CSV;
            }

            if (layer == 2 && !maps_with_bg2.Contains(mapName))
            {
                return CSV;
            }

            mapName += '_' + map_suffix[layer - 1];

            var assembly = Assembly.GetCallingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Content.csv.{mapName}.csv"))
            {
                using StreamReader reader = new StreamReader(stream);
                CSV = reader.ReadToEnd();
            }

            return CSV;
        }
    }
}
