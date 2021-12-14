using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.MapData.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.MapData
{
    public class Map
    {
        public enum Layer
        {
            BG,
            BG2,
            FG
        }

        MapLayer[] layers;

        public Map(string name)
        {
            layers = new MapLayer[] { new(), new(), new() };
            Spritesheet tiles = TileData.GetTileset(name);
            layers[0].LoadMap(MapLoader.GetMapLayer(name), tiles, Drawing.DrawOrder.MAP_BG);
            layers[1].LoadMap(MapLoader.GetMapLayer(name, 2), tiles, Drawing.DrawOrder.MAP_BG2);
            layers[2].LoadMap(MapLoader.GetMapLayer(name, 3), tiles, Drawing.DrawOrder.MAP_FG);
            TileData.SetTileProperties(name, layers[0], layers[1]);
        }

        public MapLayer GetLayer(Layer layer)
        {
            return layers[(int)layer];
        }
    }
}
