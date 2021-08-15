using AnodyneSharp.Drawing.Spritesheet;
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
        PUDDLE,
        REFLECTION,
        GRASS
    }

    public static class TileData
    {
        public static Spritesheet GetTileset(string MapName)
        {
            return new(ResourceManager.GetTexture($"{MapName.ToLower()}_tilemap", true), 16, 16);
        }

        public static void SetTileProperties(string mapName, MapLayer map, MapLayer bg2)
        {
            List<CollissionData> data = GetColData(mapName);
            SortedList<int, AnimatedTile> animationData = GetAnimData(mapName);

            map.SetAnimationData(animationData);

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
            //tile 0 never has collision in bg2(important for maps in which 0 does have collision in map, but is used as transparent in bg2)
            bg2.SetTileProperties(0, Touching.NONE, CollisionEventType.NONE, Touching.NONE);
        }

        private static SortedList<int, AnimatedTile> GetAnimData(string map)
        {
            SortedList<int, AnimatedTile> animTiles = new SortedList<int, AnimatedTile>();

            var assembly = Assembly.GetExecutingAssembly();

            foreach (var path in assembly.GetManifestResourceNames().Where(p => p.StartsWith($"{ assembly.GetName().Name}.Content.Maps.{map}.TileAnims")))
            {
                string texName = path.Split('.')[^2];

                using (Stream stream = assembly.GetManifestResourceStream(path))
                {
                    using StreamReader reader = new StreamReader(stream);

                    while (!reader.EndOfStream)
                    {
                        string[] values = reader.ReadLine().Trim().Split('\t');

                        int[] frames = values[2].Split(',').Select(v => int.Parse(v)).ToArray();

                        if (int.TryParse(values[0], out int frame) &&
                            int.TryParse(values[1], out int frameRate))
                        {
                            animTiles.Add(frame, new AnimatedTile(frames, frameRate, ResourceManager.GetTexture(texName)));
                        }
                    }
                }
            }

            return animTiles;
        }

        private static List<CollissionData> GetColData(string map)
        {
            List<CollissionData> data = new List<CollissionData>();

            var assembly = Assembly.GetExecutingAssembly();

            string path = $"{assembly.GetName().Name}.Content.Maps.{map}.TileData.col";

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
