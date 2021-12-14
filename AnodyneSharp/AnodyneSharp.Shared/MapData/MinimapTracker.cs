using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.MapData
{
    public class MinimapTracker
    {
        Dictionary<string, Minimap> minimaps = new();
        public Dictionary<string, List<int>> interest = new();

        public Minimap GetMinimap(string mapName)
        {
            if(minimaps.TryGetValue(mapName,out Minimap m))
            {
                return m;
            }
            TileMap minimap = MapLoader.GetMinimap(mapName);
            List<int> inter = interest.GetValueOrDefault(mapName);
            if(inter == null)
            {
                inter = new(Enumerable.Repeat(0, minimap.Width * minimap.Height));
                interest.Add(mapName, inter);
            }
            m = new Minimap(MapLoader.GetMinimap(mapName),inter);
            minimaps.Add(mapName, m);
            return m;
        }
    }
}
