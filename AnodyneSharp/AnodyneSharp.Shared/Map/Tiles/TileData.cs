using AnodyneSharp.Entities;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Map.Tiles
{
    public enum CollisionEventType
    {
        NONE,
        CONVEYOR,
        THIN,
        HOLE,
        SLOW,
        SPIKE,
        LADDER
    }

    public static class TileData
    {
        private static SortedList<string, string> TileMaps;

        public static int Overworld_Tileset_Width = 10;
        public static int Overworld_Tilemap_Solid_Rows = 4;
        public static int Overworld_Tilemap_Special_Row_offset = 4;
        public static int Overworld_Tilemap_Nonsolid_Row_offset = 5;
        public static int Overworld_Tilemap_Nr_Rows = 10;

        public static Texture2D Tiles;

        static TileData()
        {
            TileMaps = new SortedList<string, string>();

            List<string> paths = new List<string>()
            {
                "debug_tilemap",
                "apartment_tilemap",
                "bedroom_tilemap",
                "fields_tilemap",
                "street_tilemap",
                "overworld_tilemap",
                "beach_tilemap",
                "blank_tilemap",
                "cliff_tilemap",
                "crowd_tilemap",
                "circus_tilemap",
                "forest_tilemap",
                "hotel_tilemap",
                "redsea_tilemap",
                "redcave_tilemap",
                "nexus_tilemap",
                "windmill_tilemap",
                "terminal_tilemap",
                "train_tilemap",
                "blackwhite_tilemap",
                "blue_tilemap",
                "happy_tilemap",
                "space_tilemap",
                "suburb_tilemap",
                "go_tilemap"
            };

            foreach (var path in paths)
            {
                TileMaps.Add(path.Split('_').First().ToUpper(), path);
            }

        }

        public static void SetTileset(string MapName)
        {
            if (TileMaps.ContainsKey(MapName))
            {
                Tiles = ResourceManager.GetTexture(TileMaps[MapName]);
            }
            else
            {
                Tiles = ResourceManager.GetTexture(TileMaps["DEBUG"]);
            }
        }

        public static void Set_tile_properties(TileMap map)
        {
            if (GlobalState.CURRENT_MAP_NAME == "STREET")
            {
                map.SetTileProperties(29, Touching.NONE, tileMax: 80);
            }
        }
    }
}
