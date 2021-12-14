using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Entities;
using AnodyneSharp.MapData.Tiles;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.MapData
{
    public class MapLayer
    {
        public int HeightInTiles => Data.Height;
        public int WidthInTiles => Data.Width;

        private Rectangle?[] _rects;

        public TileMap Data { get; protected set; }
        protected SortedList<int, AnimatedTile> animatedTiles;

        protected Spritesheet tiles;
        private Tile[] _tileObjects;

        private DrawOrder _layer;
        private string textureName;

        public void LoadMap(TileMap mapData, Spritesheet tileMap, DrawOrder layer)
        {
            textureName = tileMap.Tex.Name.Split('/').Last();

            Data = mapData;

            tiles = tileMap;
            animatedTiles = new SortedList<int, AnimatedTile>();

            _layer = layer;

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
                tile.direction = direction;
            }
        }

        public Vector2 GetFirstWalkable(MapLayer layered)
        {
            //1 to not end up on the top row everywhere
            for (int y = 1; y < HeightInTiles; ++y)
            {
                for (int x = 0; x < WidthInTiles; ++x)
                {
                    int tile = Data.GetTile(new(x, y));
                    if (tile != 0 && _tileObjects[tile].allowCollisions == Touching.NONE && layered._tileObjects[layered.Data.GetTile(new(x, y))].allowCollisions == Touching.NONE)
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

                    Tile t = _tileObjects[Data.GetTile(new(x, y))];

                    t.lastPosition.X = t.Position.X = x * GameConstants.TILE_WIDTH;
                    t.lastPosition.Y = t.Position.Y = y * GameConstants.TILE_HEIGHT;

                    if (t.allowCollisions == Touching.NONE || GameObject.Separate(ent, t))
                    {
                        if (t.collisionEventType != CollisionEventType.NONE && ent.MapInteraction)
                        {
                            switch (t.collisionEventType)
                            {
                                case CollisionEventType.CONVEYOR:
                                    if(t.Hitbox.Contains(ent.Center))
                                    {
                                        ent.Conveyor(t.direction);
                                    }
                                    break;
                                case CollisionEventType.THIN:
                                    {
                                        GameObject collider = t.direction switch
                                        {
                                            Touching.DOWN => new(t.Position + new Vector2(0, 13), t.width, 3),
                                            Touching.UP => new(t.Position, t.width, 3),
                                            Touching.LEFT => new(t.Position, 3, t.height),
                                            Touching.RIGHT => new(t.Position + new Vector2(13, 0), 3, t.height),
                                            _ => null,
                                        };
                                        collider.immovable = true;
                                        GameObject.Separate(ent, collider);
                                    }
                                    break;
                                case CollisionEventType.HOLE:
                                    {
                                        Rectangle actualHitbox = t.Hitbox;
                                        actualHitbox.Y += 5; actualHitbox.Height = 4;
                                        actualHitbox.X += 5; actualHitbox.Width = 6;

                                        if (actualHitbox.Intersects(ent.Hitbox))
                                        {
                                            ent.Fall(t.Position);
                                        }
                                    }
                                    break;
                                case CollisionEventType.SLOW:
                                    ent.SlowTile();
                                    break;
                                case CollisionEventType.SPIKE:
                                    {
                                        Rectangle actualHitbox = t.Hitbox;
                                        actualHitbox.Y += 6; actualHitbox.Height = 6;
                                        actualHitbox.X += 6; actualHitbox.Width = 5;
                                        if (ent is Player p && p.state != PlayerState.AIR && actualHitbox.Intersects(p.Hitbox)) p.ReceiveDamage(1, false);
                                    }
                                    break;
                                case CollisionEventType.LADDER:
                                    ent.Ladder();
                                    break;
                                case CollisionEventType.PUDDLE:
                                    if (t.Hitbox.Contains(ent.Center))
                                    {
                                        ent.Puddle();
                                    }
                                    break;
                                case CollisionEventType.REFLECTION:
                                    if (t.Hitbox.Contains(ent.Center))
                                    {
                                        ent.Puddle();
                                        ent.Reflection();
                                    }
                                    break;
                                case CollisionEventType.GRASS:
                                    if(t.Hitbox.Contains(ent.Center))
                                    {
                                        ent.Grass();
                                    }
                                    break;
                            }
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

        public void Draw(Rectangle bounds, bool ignoreEmpty = false)
        {
            float z = DrawingUtilities.GetDrawingZ(_layer);

            for (int y = bounds.Y / GameConstants.TILE_HEIGHT - 1; y < bounds.Bottom / GameConstants.TILE_HEIGHT + 1; y++)
            {
                for (int x = bounds.X / GameConstants.TILE_WIDTH - 1; x < bounds.Right / GameConstants.TILE_WIDTH + 1; x++)
                {
                    int tile = Data.GetTile(new(x,y));
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

            return _tileObjects[Data.GetTile(position.ToPoint())].allowCollisions;
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
