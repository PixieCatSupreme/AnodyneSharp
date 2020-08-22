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
        public static int Overworld_Tileset_Width = 10;
        public static int Overworld_Tilemap_Solid_Rows = 4;
        public static int Overworld_Tilemap_Special_Row_offset = 4;
        public static int Overworld_Tilemap_Nonsolid_Row_offset = 5;
        public static int Overworld_Tilemap_Nr_Rows = 10;

        public static Texture2D Tiles;

        public static void SetTileset(string MapName)
        {
            Tiles = ResourceManager.GetTexture($"{MapName.ToLower()}_tilemap", true);

            if (Tiles == null)
            {
                Tiles = ResourceManager.GetTexture("debug_tilemap", true);
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
                    map.SetTileProperties(d.Start, d.AllowedCollisions, d.CollisionEventType, d.Direction, tileMax: d.End.Value);
                    bg2.SetTileProperties(d.Start, d.AllowedCollisions, d.CollisionEventType, d.Direction, tileMax: d.End.Value);
                }
            }
        }

        private static List<CollissionData> GetColData()
        {
            List<CollissionData> data = new List<CollissionData>();

            var assembly = Assembly.GetExecutingAssembly();

            string path = $"{assembly.GetName().Name}.Content.Maps.{GlobalState.CURRENT_MAP_NAME}.TileData.col";

            using (Stream stream = assembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    return new List<CollissionData>();
                }

                using StreamReader reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    string[] dataStrings = reader.ReadLine().Split('\t');
                    CollisionEventType eventType = CollisionEventType.NONE;
                    Touching direction = Touching.ANY;

                    if (Enum.TryParse(dataStrings[1], out Touching allowedCol) &&
                        (dataStrings.Length == 2 || (Enum.TryParse(dataStrings[2], out eventType) &&
                        (dataStrings.Length <= 3 || Enum.TryParse(dataStrings[3], out direction)))))
                    {
                        if (dataStrings[0].Contains('-'))
                        {
                            string[] minMax = dataStrings[0].Split('-');

                            if (int.TryParse(minMax[0], out int min) &&
                                int.TryParse(minMax[1], out int max))
                            {
                                data.Add(new CollissionData(min, max, allowedCol, eventType, direction));
                            }
                        }
                        else
                        {
                            if (int.TryParse(dataStrings[0], out int min))
                            {
                                data.Add(new CollissionData(min, allowedCol, eventType, direction));
                            }
                        }
                    }
                }
            }

            return data;
        }
    }
}
