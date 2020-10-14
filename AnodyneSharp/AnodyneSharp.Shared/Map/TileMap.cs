using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Map
{
    public class TileMap
    {
        private List<int> tiles;
        public readonly int Width;
        public readonly int Height;

        public TileMap(string data)
        {
            tiles = new List<int>();
            string[] rows = data.Trim().Split('\n');
            Height = rows.Length;

            foreach (string row in rows)
            {
                int[] columns = row.Split(',').Select(int.Parse).ToArray();
                if(Width != 0 && Width != columns.Length)
                {
                    throw new FormatException($"Inconsistent row length {Width} vs {columns.Length}!");
                }
                Width = columns.Length;
                tiles.AddRange(columns);
            }
        }

        public int GetTile(int x, int y)
        {
            if (x >= Width || x < 0 || y < 0 || y >= Height)
                return 0; //TODO implement PUs
            return tiles[x + y * Width];
        }
    }
}
