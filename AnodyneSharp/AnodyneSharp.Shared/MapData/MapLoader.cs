using AnodyneSharp.Logging;
using AnodyneSharp.Utilities;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AnodyneSharp.MapData
{
    public static class MapLoader
    {
        private enum Layer
        {
            BG = 1,
            BG2,
            FG
        }

        private static TileMap LoadMap(string path, string defaultCSV, bool crit = true)
        {
            string CSV = defaultCSV;

            using (Stream stream = AssemblyReaderUtil.GetStream(path))
            {
                if (stream == null)
                {
                    if (crit)
                    {
                        DebugLogger.AddCritical($"Unable to find map at {path}");
                    }
                    return new TileMap(CSV);
                }
                using StreamReader reader = new StreamReader(stream);
                CSV = reader.ReadToEnd();
            }

            return new TileMap(CSV);
        }

        public static TileMap GetMinimap(string mapName)
        {
            string path = $"Content.MiniMaps.Minimap_{mapName}.csv";

            return LoadMap(path,"",false);
        }

        public static TileMap GetMapLayer(string mapName, int layer = 1)
        {
            string path = $"Content.Maps.{mapName}.{(Layer)layer}.csv";

            return LoadMap(path,"0,0",layer == 1);
        }
    }
}
