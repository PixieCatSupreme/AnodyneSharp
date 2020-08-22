using AnodyneSharp.Logging;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AnodyneSharp.Map
{
    public static class MapLoader
    {
        private enum Layer
        {
            BG = 1,
            BG2,
            FG
        }

        public static string GetMap(string mapName, int layer = 1)
        {
            string CSV = "0,0";

            var assembly = Assembly.GetCallingAssembly();

            string path = $"{assembly.GetName().Name}.Content.Maps.{mapName}.{(Layer)layer}.csv";

            if (layer != 1 && !assembly.GetManifestResourceNames().Contains(path))
            {
                return CSV;
            }

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    DebugLogger.AddCritical($"Unable to find map at {path}");
                    return CSV;
                }
                using StreamReader reader = new StreamReader(stream);
                CSV = reader.ReadToEnd();
            }

            return CSV;
        }
    }
}
