using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Player;
using AnodyneSharp.Map.Tiles;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Map
{
    public class TileMap
    {
        public float width;
        public float height;
        private Rectangle?[] _rects;
        protected int heightInTiles;
        protected int widthInTiles;
        protected List<int> data;
        protected int totalTiles;

        protected Texture2D tiles;
        private int _tileWidth;
        private int _tileHeight;
        private Tile[] _tileObjects;

        private DrawOrder _layer;

        /**
		 * Y position of the upper left corner of this object in world space.
		 */
        public float y;

        public void LoadMap(string mapData, Texture2D tileMap, DrawOrder layer)
        {
            //Figure out the map dimensions based on the data string
            string[] columns;
            string[] rows = mapData.Split('\n');
            heightInTiles = rows.Length;
            widthInTiles = 0;
            data = new List<int>();

            tiles = tileMap;

            _layer = layer;

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
            _tileWidth = GameConstants.TILE_WIDTH;
            if (_tileWidth == 0)
            {
                _tileWidth = tiles.Height;
            }

            _tileHeight = GameConstants.TILE_HEIGHT;
            if (_tileHeight == 0)
            {
                _tileHeight = _tileWidth;
            }


            //create some tile objects that we'll use for overlap checks (one for each tile)
            int l = (tiles?.Width ?? 0) / _tileWidth * (tiles?.Height ?? 0) / _tileHeight;

            _tileObjects = new Tile[l];

            for (int i = 0; i < l; i++)
            {
                //Tile 0 is invisible and has no collision, all others default to being visible and being collidable from all directions.
                //Default gets overridden by SetTileProperties later on
                _tileObjects[i] = new Tile(_tileWidth, _tileHeight, (i >= 1), (i >= 1) ? Touching.ANY : Touching.NONE);
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

        internal void SetTileProperties(int tileMin, Touching allowCollisions, CollisionEventType collisionEventType, Touching direction, int tileMax = 0)
        {
            if (tileMax == 0)
            {
                tileMax = tileMin;
            }

            for (int i = tileMin; i <= tileMax; i++)
            {
                Tile tile = _tileObjects[i];

                tile.allowCollisions = allowCollisions;
                tile.collisionEventType = collisionEventType;
            }
        }

        public Vector2 GetFirstWalkable(TileMap layered)
        {
            int loc = data.FindIndex(i => i != 0 && _tileObjects[i].allowCollisions == Touching.NONE && layered._tileObjects[i].allowCollisions == Touching.NONE);
            if (loc == -1)
                return Vector2.Zero;
            else
                return new Vector2(loc % widthInTiles, loc / widthInTiles);
        }

        public void Collide(Entity ent, bool onlyCurrentScreen = false)
        {
            Rectangle hitbox = ent.Hitbox;
            for(int y = hitbox.Top/GameConstants.TILE_WIDTH; y <= hitbox.Bottom/GameConstants.TILE_WIDTH; ++y)
            {
                if (y < 0) continue;
                if (y >= heightInTiles) break;
                if (onlyCurrentScreen && y / GameConstants.SCREEN_HEIGHT_IN_TILES != GlobalState.CURRENT_GRID_Y) continue;
                for (int x = hitbox.Left / GameConstants.TILE_WIDTH; x <= hitbox.Right / GameConstants.TILE_WIDTH; ++x)
                {
                    if (x < 0) continue;
                    if (x >= widthInTiles) break;
                    if (onlyCurrentScreen && x / GameConstants.SCREEN_WIDTH_IN_TILES != GlobalState.CURRENT_GRID_X) continue;

                    Tile t = _tileObjects[data[x + y * widthInTiles]];

                    t.lastPosition.X = t.Position.X = x * GameConstants.TILE_WIDTH;
                    t.lastPosition.Y = t.Position.Y = y * GameConstants.TILE_HEIGHT;

                    if(t.allowCollisions == Touching.NONE || GameObject.Separate(ent,t))
                    {
                        if(t.collisionEventType != CollisionEventType.NONE)
                        {
                            //Call event method in TileData
                        }
                    }

                }
            }

        }

        public void Draw(bool ignoreEmpty = false)
        {
            float z = DrawingUtilities.GetDrawingZ(_layer);

            for (int y = 0; y < heightInTiles; y++)
            {
                for (int x = 0; x < widthInTiles; x++)
                {
                    Rectangle? rect = _rects[x + y * widthInTiles];

                    if (rect.HasValue)
                    {
                        int tile = data[x + y * widthInTiles];

                        if (!ignoreEmpty || tile != 0)
                        {
                            SpriteDrawer.DrawSprite(tiles, new Rectangle(x * _tileWidth, y * _tileHeight, _tileWidth, _tileHeight), rect.Value, Z: z);
                        }
                    }
                }
            }
        }

        public Touching GetCollisionData(Vector2 position)
        {
            position /= GameConstants.TILE_HEIGHT;
            if (position.X < 0 || position.X >= widthInTiles || position.Y < 0 || position.Y >= heightInTiles)
                return Touching.NONE;

            return _tileObjects[data[(int)position.X + (int)position.Y * widthInTiles]].allowCollisions;
        }

        protected void UpdateTile(int Index)
        {
            var d = data[Index];

            if (_tileObjects.Length == 0 || (!(_tileObjects[d] is Tile tile)) || !tile.visible)
            {
                _rects[Index] = null;
                return;
            }

            int rx = d * _tileWidth;
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
