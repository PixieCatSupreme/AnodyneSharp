using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Map.Tiles;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Map
{
    public class TileMap
    {
        public int HeightInTiles { get; protected set; }
        public int WidthInTiles { get; protected set; }

        public float width;
        public float height;
        private Rectangle?[] _rects;

        protected List<int> data;
        protected SortedList<int, AnimatedTile> animatedTiles;

        protected int totalTiles;

        protected Texture2D tiles;
        private int _tileWidth;
        private int _tileHeight;
        private Tile[] _tileObjects;

        private DrawOrder _layer;
        private string textureName;

        /**
		 * Y position of the upper left corner of this object in world space.
		 */
        public float y;

        public void LoadMap(string mapData, Texture2D tileMap, DrawOrder layer)
        {
            textureName = tileMap.Name.Split('/').Last();

            //Figure out the map dimensions based on the data string
            string[] columns;
            string[] rows = mapData.Split('\n');
            HeightInTiles = rows.Length;
            WidthInTiles = 0;
            data = new List<int>();

            tiles = tileMap;
            animatedTiles = new SortedList<int, AnimatedTile>();

            _layer = layer;

            uint row = 0;
            while (row < HeightInTiles)
            {
                columns = rows[row++].Split(',');
                if (columns.Length <= 1)
                {
                    HeightInTiles -= 1;
                    continue;
                }
                if (WidthInTiles == 0)
                    WidthInTiles = columns.Length;
                uint column = 0;
                while (column < WidthInTiles)
                    data.Add(int.Parse(columns[column++]));
            }

            totalTiles = WidthInTiles * HeightInTiles;

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
            width = WidthInTiles * _tileWidth;
            height = HeightInTiles * _tileHeight;
            _rects = new Rectangle?[totalTiles];
            for (int i = 0; i < totalTiles; i++)
            {
                UpdateTile(i);
            }
        }

        internal void SetAnimationData(SortedList<int, AnimatedTile> animationData)
        {
            animatedTiles = animationData;
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
                return new Vector2(loc % WidthInTiles, loc / WidthInTiles);
        }

        public void Collide(Entity ent, bool onlyCurrentScreen = false)
        {
            Rectangle hitbox = ent.Hitbox;
            for (int y = hitbox.Top / GameConstants.TILE_WIDTH; y <= hitbox.Bottom / GameConstants.TILE_WIDTH; ++y)
            {
                if (y < 0) continue;
                if (y >= HeightInTiles) break;
                if (onlyCurrentScreen && y / GameConstants.SCREEN_HEIGHT_IN_TILES != GlobalState.CURRENT_GRID_Y) continue;
                for (int x = hitbox.Left / GameConstants.TILE_WIDTH; x <= hitbox.Right / GameConstants.TILE_WIDTH; ++x)
                {
                    if (x < 0) continue;
                    if (x >= WidthInTiles) break;
                    if (onlyCurrentScreen && x / GameConstants.SCREEN_WIDTH_IN_TILES != GlobalState.CURRENT_GRID_X) continue;

                    Tile t = _tileObjects[data[x + y * WidthInTiles]];

                    t.lastPosition.X = t.Position.X = x * GameConstants.TILE_WIDTH;
                    t.lastPosition.Y = t.Position.Y = y * GameConstants.TILE_HEIGHT;

                    if (t.allowCollisions == Touching.NONE || GameObject.Separate(ent, t))
                    {
                        if (t.collisionEventType != CollisionEventType.NONE)
                        {
                            //TODO: Call event method in TileData
                        }
                    }

                }
            }

        }

        public void Update()
        {
            foreach (var tile in animatedTiles.Values)
            {
                tile.UpdateAnimation();
            }
        }

        public void Draw(bool ignoreEmpty = false)
        {
            float z = DrawingUtilities.GetDrawingZ(_layer);

            for (int y = 0; y < HeightInTiles; y++)
            {
                for (int x = 0; x < WidthInTiles; x++)
                {
                    Rectangle? rect = _rects[x + y * WidthInTiles];

                    if (rect.HasValue)
                    {
                        int tile = data[x + y * WidthInTiles];

                        if (!ignoreEmpty || tile != 0)
                        {
                            Texture2D tex = tiles;
                            Rectangle source = rect.Value;

                            if (animatedTiles.TryGetValue(tile, out AnimatedTile animTile))
                            {
                                tex = animTile.Texture;
                                source = animTile.spriteRect;
                            }

                            SpriteDrawer.DrawSprite(tex, new Rectangle(x * _tileWidth, y * _tileHeight, _tileWidth, _tileHeight), source, Z: z);
                        }
                    }
                }
            }
        }

        public Touching GetCollisionData(Vector2 position)
        {
            position /= GameConstants.TILE_HEIGHT;
            if (position.X < 0 || position.X >= WidthInTiles || position.Y < 0 || position.Y >= HeightInTiles)
                return Touching.NONE;

            return _tileObjects[data[(int)position.X + (int)position.Y * WidthInTiles]].allowCollisions;
        }

        public void ReloadTexture()
        {
            tiles = ResourceManager.GetTexture(textureName);
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
