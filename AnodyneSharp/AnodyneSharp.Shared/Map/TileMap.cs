using AnodyneSharp.Map.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AnodyneSharp.Map
{
    [Flags]
    public enum CollissionDirections
    {
        NONE,
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
    public class TileMap
    {
        public CollissionDirections allowCollisions;

        public double width;
		public double height;
        private Rectangle?[] _rects;
        protected int heightInTiles;
        protected int widthInTiles;
        protected List<int> data;
        protected int totalTiles;

        protected Texture2D tiles;
        private int _tileWidth;
        private int _tileHeight;
        private Tile[] _tileObjects;

        public void LoadMap(string mapData, Texture2D tileMap, int tileWidth = 16, int tileHeight = 16)
        {
            //Figure out the map dimensions based on the data string
            string[] columns;
            string[] rows = mapData.Split('\n');
            heightInTiles = rows.Length;
            widthInTiles = 0;
            data = new List<int>();

            uint row = 0;
            uint column = 0;
            while (row < heightInTiles)
            {
                columns = rows[row++].Split(',');
                if (columns.Length <= 1)
                {
                    heightInTiles = heightInTiles - 1;
                    continue;
                }
                if (widthInTiles == 0)
                    widthInTiles = columns.Length;
                column = 0;
                while (column < widthInTiles)
                    data.Add(int.Parse(columns[column++]));
            }

            totalTiles = widthInTiles * heightInTiles;

            //Figure out the size of the tiles
            tiles = tileMap;
            _tileWidth = tileWidth;
            if (_tileWidth == 0)
            {
                _tileWidth = tiles.Height;
            }

            _tileHeight = tileHeight;
            if (_tileHeight == 0)
            {
                _tileHeight = _tileWidth;
            }


            //create some tile objects that we'll use for overlap checks (one for each tile)
            int l = (tiles.Width / _tileWidth) * (tiles.Height / _tileHeight);

            _tileObjects = new Tile[l];

            for (int i = 0; i < l; i++)
            {
                _tileObjects[i] = new Tile(this, i, _tileWidth, _tileHeight, (i >= 1), (i >= 1) ? allowCollisions : CollissionDirections.NONE);
            }

            //Then go through and create the actual map
            width = widthInTiles * _tileWidth;
            height = heightInTiles * _tileHeight;
            _rects = new Rectangle?[totalTiles];
            for (int i = 0; i < totalTiles; i++)
            {
                UpdateTile(i);
            }
        }

        protected void UpdateTile(int Index)
		{
			Tile tile = _tileObjects[data[Index]] as Tile;
			if((tile == null) || !tile.visible)
			{
				_rects[Index] = null;
				return;
			}

            int rx = data[Index] * _tileWidth;
            int ry = 0;

            if (rx >= tiles.Width)
            {
                ry = (rx / tiles.Width) * _tileHeight;
                rx %= tiles.Width;
            }
            _rects[Index] = new Rectangle(rx, ry, _tileWidth, _tileHeight);
		}
    }
}
