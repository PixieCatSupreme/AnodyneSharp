using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.MapData
{
    public class TileMap
    {
        private List<int> tiles;
        public readonly int Width;
        public readonly int Height;

        private Dictionary<Point, int> changedTiles = new();
        private Dictionary<Point, int> oldChangedTiles = new();

        public TileMap(string data)
        {
            tiles = new List<int>();
            if (data == "") return;
            string[] rows = data.Trim().Split('\n');
            Height = rows.Length;

            foreach (string row in rows)
            {
                int[] columns = row.Split(',').Select(int.Parse).ToArray();
                if (Width != 0 && Width != columns.Length)
                {
                    throw new FormatException($"Inconsistent row length {Width} vs {columns.Length}!");
                }
                Width = columns.Length;
                tiles.AddRange(columns);
            }
        }

        public void OnTransitionStart()
        {
            oldChangedTiles = changedTiles;
            changedTiles = new();
        }

        public void OnTransitionEnd()
        {
            foreach((Point loc, int old) in oldChangedTiles)
            {
                tiles[loc.X + loc.Y * Width] = old;
            }
        }

        public void ChangeTile(Point loc, int new_val)
        {
            int pos = loc.X + loc.Y * Width;
            if(pos < 0 || pos >= tiles.Count)
            {
                return;
            }
            if(!changedTiles.ContainsKey(loc))
            {
                changedTiles.Add(loc, GetTile(loc.X, loc.Y));
            }
            tiles[pos] = new_val;
        }

        public int GetTile(int x, int y)
        {
            int pos = x + y * Width;
            if (pos < 0 || pos >= tiles.Count)
                return 0;
            return tiles[pos];
        }
    }
}
