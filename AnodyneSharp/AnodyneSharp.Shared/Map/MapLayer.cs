using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
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
    public class MapLayer
    {
        public int HeightInTiles => data.Height;
        public int WidthInTiles => data.Width;

        public float width;
        public float height;
        private Rectangle?[] _rects;

        protected TileMap data;
        protected SortedList<int, AnimatedTile> animatedTiles;

        protected Spritesheet tiles;
        private Tile[] _tileObjects;

        private DrawOrder _layer;
        private string textureName;

        public void LoadMap(TileMap mapData, Spritesheet tileMap, DrawOrder layer)
        {
            textureName = tileMap.Tex.Name.Split('/').Last();

            data = mapData;

            tiles = tileMap;
            animatedTiles = new SortedList<int, AnimatedTile>();

            _layer = layer;

            width = WidthInTiles * tiles.Width;
            height = HeightInTiles * tiles.Height;

            //create some tile objects that we'll use for overlap checks (one for each tile)
            _tileObjects = new Tile[tiles.NumFrames];

            for (int i = 0; i < _tileObjects.Length; i++)
            {
                //Tile 0 is invisible and has no collision, all others default to being visible and being collidable from all directions.
                //Default gets overridden by SetTileProperties later on
                _tileObjects[i] = new Tile(tiles.Width, tiles.Height, (i >= 1), (i >= 1) ? Touching.ANY : Touching.NONE);
            }
            
            _rects = new Rectangle?[tiles.NumFrames];
            for (int i = 0; i < _rects.Length; i++)
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

        public Vector2 GetFirstWalkable(MapLayer layered)
        {
            //1 to not end up on the top row everywhere
            for (int y = 1; y < HeightInTiles; ++y)
            {
                for (int x = 0; x < WidthInTiles; ++x)
                {
                    int tile = data.GetTile(x, y);
                    if (tile != 0 && _tileObjects[tile].allowCollisions == Touching.NONE && layered._tileObjects[layered.data.GetTile(x, y)].allowCollisions == Touching.NONE)
                        return new Vector2(x, y);
                }
            }
            return Vector2.Zero;
        }

        public void Collide(Entity ent, bool onlyCurrentScreen = false)
        {
            Rectangle hitbox = ent.Hitbox;
            for (int y = hitbox.Top / GameConstants.TILE_WIDTH; y <= hitbox.Bottom / GameConstants.TILE_WIDTH; ++y)
            {
                if (onlyCurrentScreen && y / GameConstants.SCREEN_HEIGHT_IN_TILES != GlobalState.CURRENT_GRID_Y) continue;
                for (int x = hitbox.Left / GameConstants.TILE_WIDTH; x <= hitbox.Right / GameConstants.TILE_WIDTH; ++x)
                {
                    if (onlyCurrentScreen && x / GameConstants.SCREEN_WIDTH_IN_TILES != GlobalState.CURRENT_GRID_X) continue;

                    Tile t = _tileObjects[data.GetTile(x, y)];

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
                    int tile = data.GetTile(x, y);
                    Rectangle? rect = _rects[tile];

                    if (rect.HasValue)
                    {
                        if (!ignoreEmpty || tile != 0)
                        {
                            Texture2D tex = tiles.Tex;
                            Rectangle source = rect.Value;

                            if (animatedTiles.TryGetValue(tile, out AnimatedTile animTile))
                            {
                                tex = animTile.sprite.Tex;
                                source = animTile.spriteRect;
                            }

                            SpriteDrawer.DrawSprite(tex, new Rectangle(x * tiles.Width, y * tiles.Height, tiles.Width, tiles.Height), source, Z: z);
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

            return _tileObjects[data.GetTile((int)position.X, (int)position.Y)].allowCollisions;
        }

        public void ReloadTexture()
        {
            tiles = new Spritesheet(ResourceManager.GetTexture(textureName), tiles.Width, tiles.Height);
        }

        protected void UpdateTile(int Index)
        {
            if (!_tileObjects[Index].visible)
            {
                _rects[Index] = null;
                return;
            }

            _rects[Index] = tiles.GetRect(Index);
        }
    }
}
