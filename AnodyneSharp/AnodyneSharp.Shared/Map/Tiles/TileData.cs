using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Map.Tiles
{
    public static class TileData
    {
        private static SortedList<string, Texture2D> TileMaps;

        public static int Overworld_Tileset_Width = 10;
        public static int Overworld_Tilemap_Solid_Rows = 4;
        public static int Overworld_Tilemap_Special_Row_offset = 4;
        public static int Overworld_Tilemap_Nonsolid_Row_offset = 5;
        public static int Overworld_Tilemap_Nr_Rows = 10;

        public static Texture2D Tiles;

        static TileData()
        {
            TileMaps = new SortedList<string, Texture2D>();
        }

        public static void LoadTileMaps(ContentManager content)
        {
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
                "redsea_tiles",
                "redcave_tiles",
                "nexus_tilemap",
                "windmill_tilemap",
                "terminal_tilemap",
                "cell_tilemap",
                "blackwhite_tilemap",
                "blue_tilemap",
                "happy_tilemap",
                "space_tilemap",
                "suburb_tilemap",
                "go_tilemap"
            };

            foreach (var path in paths)
            {
                TileMaps.Add(path.Split('_').First().ToUpper(), LoadTexture(path, content));
            }
        }

        public static void SetTileset(string MapName)
        {
            if (TileMaps.ContainsKey(MapName))
            {
                Tiles = TileMaps[MapName];
            }
            else
            {
                Tiles = TileMaps["DEBUG"];
            }
        }

        private static Texture2D LoadTexture(string path, ContentManager content)
        {
            return content.Load<Texture2D>($"tilemaps/{path}");
        }
    }
}
