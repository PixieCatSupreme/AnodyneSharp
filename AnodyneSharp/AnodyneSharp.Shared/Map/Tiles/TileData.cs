using AnodyneSharp.Entities;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        LADDER,
        PUDDLE
    }

    public static class TileData
    {
        private const string CollissionFilePath = "Content.tileProperties";

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

        public static void Set_tile_properties(TileMap map, TileMap bg2)
        {
            List<CollissionData> data = GetColData();

            foreach (var d in data)
            {
                if (d.End == null)
                {
                    map.SetTileProperties(d.Start, d.AllowedCollisions, d.CollisionEventType, d.Direction);
                    bg2.SetTileProperties(d.Start, d.AllowedCollisions, d.CollisionEventType, d.Direction);
                }
                else
                {
                    map.SetTileProperties(d.Start, d.AllowedCollisions, d.CollisionEventType,d.Direction, tileMax: d.End.Value);
                    bg2.SetTileProperties(d.Start, d.AllowedCollisions, d.CollisionEventType, d.Direction, tileMax: d.End.Value);
                }
            }
        }

        private static List<CollissionData> GetColData()
        {
            List<CollissionData> data = new List<CollissionData>();

            var assembly = Assembly.GetExecutingAssembly();

            string path = $"{assembly.GetName().Name}.{CollissionFilePath}.{GlobalState.CURRENT_MAP_NAME}.col";

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while(!reader.EndOfStream)
                    {
                        string[] dataStrings = reader.ReadLine().Split('\t');

                        int min = -1;
                        int max = -1;
                        Touching allowedCol = Touching.ANY;
                        CollisionEventType eventType = CollisionEventType.NONE;
                        Touching direction = Touching.ANY;

                        if (Enum.TryParse(dataStrings[1], out allowedCol) &&
                            (dataStrings.Length == 2 || (Enum.TryParse(dataStrings[2], out eventType) && 
                            (dataStrings.Length <= 3 || Enum.TryParse(dataStrings[3], out direction)))))
                        {
                            if (dataStrings[0].Contains('-'))
                            {
                                string[] minMax = dataStrings[0].Split('-');

                                if (int.TryParse(minMax[0], out min) &&
                                    int.TryParse(minMax[1], out max))
                                {
                                    data.Add(new CollissionData(min, max, allowedCol, eventType, direction));
                                }
                            }
                            else
                            {
                                if (int.TryParse(dataStrings[0], out min))
                                {
                                    data.Add(new CollissionData(min, allowedCol, eventType, direction));
                                }
                            }
                        }
                    }
                }
            }

            return data;
        }
    }
}
