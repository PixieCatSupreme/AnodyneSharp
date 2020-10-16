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

        public static TileMap LoadMap(string path)
        {
            string CSV = "0,0";

            using (Stream stream = Assembly.GetCallingAssembly().GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    DebugLogger.AddCritical($"Unable to find map at {path}");
                    return new TileMap(CSV);
                }
                using StreamReader reader = new StreamReader(stream);
                CSV = reader.ReadToEnd();
            }

            return new TileMap(CSV);
        }

        public static TileMap GetMinimap(string mapName)
        {
            var assembly = Assembly.GetCallingAssembly();

            string path = $"{assembly.GetName().Name}.Content.MiniMaps.Minimap_{mapName}.csv";

            if(!assembly.GetManifestResourceNames().Contains(path))
            {
                return new TileMap("");
            }

            return LoadMap(path);
        }

        public static TileMap GetMapLayer(string mapName, int layer = 1)
        {
            string CSV = "0,0";

            var assembly = Assembly.GetCallingAssembly();

            string path = $"{assembly.GetName().Name}.Content.Maps.{mapName}.{(Layer)layer}.csv";

            if (layer != 1 && !assembly.GetManifestResourceNames().Contains(path))
            {
                return new TileMap(CSV);
            }

            return LoadMap(path);
        }
    }
}
