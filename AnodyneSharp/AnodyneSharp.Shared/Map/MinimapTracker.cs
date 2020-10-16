using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Map
{
    public class MinimapTracker
    {
        Dictionary<string, Minimap> minimaps = new Dictionary<string, Minimap>();

        public MinimapTracker() { }

        public Minimap GetMinimap(string mapName)
        {
            if(minimaps.TryGetValue(mapName,out Minimap m))
            {
                return m;
            }
            m = new Minimap(MapLoader.GetMinimap(mapName));
            minimaps.Add(mapName, m);
            return m;
        }
    }
}
