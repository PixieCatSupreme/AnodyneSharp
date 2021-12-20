using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Entities;
using AnodyneSharp.MapData.Tiles;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AnodyneSharp.MapData
{
    public enum Layer
    {
        BG,
        BG2,
        FG
    }

    public interface IPublicMap
    {
        int GetTile(Layer layer, Point pos);
        void ChangeTile(Layer layer, Point pos, int newVal);
        Point ToMapLoc(Vector2 pos);
        Touching GetCollisionData(Vector2 pos);
        SwapperControl.State CheckSwapper(Point tile);
    }

    public class Map : IPublicMap
    {
        TileMap[] mapLayers;

        public int WidthInTiles => mapLayers[0].Width;
        public int HeightInTiles => mapLayers[0].Height;

        public Vector2 offset = new();

        private Spritesheet _tiles;
        private SortedList<int, AnimatedTile> _animatedTiles;
        private Tile[] _tileObjects;

        private SwapperControl swapper;

        private Settings.MapSettings settings;

        private string mapName;

        public Map(string name)
        {
            mapName = name;
            mapLayers = new TileMap[]
            {
                MapLoader.GetMapLayer(name),
                MapLoader.GetMapLayer(name,2),
                MapLoader.GetMapLayer(name,3)
            };
            _tiles = TileData.GetTileset(name);
            _animatedTiles = TileData.GetAnimData(name);
            _tileObjects = new Tile[_tiles.NumFrames];
            for (int i = 0; i < _tileObjects.Length; i++)
            {
                //Tile 0 is invisible and has no collision, all others default to being visible and being collidable from all directions.
                //Default gets overridden by SetTileProperties later on
                _tileObjects[i] = new Tile(_tiles.Width, _tiles.Height, (i >= 1), (i >= 1) ? Touching.ANY : Touching.NONE);
            }
            TileData.SetTileProperties(name,_tileObjects);

            swapper = new(name);

            using Stream settingsStream = Utilities.AssemblyReaderUtil.GetStream($"Content.Maps.{name}.Settings.json");
            if(settingsStream != null)
            {
                using var reader = new StreamReader(settingsStream);
                settings = JsonSerializer.Deserialize<Settings.MapSettings>(reader.ReadToEnd());
            }
        }

        public void Draw(Rectangle bounds)
        {
            DrawLayer(bounds, mapLayers[0], DrawOrder.MAP_BG);
            DrawLayer(bounds, mapLayers[1], DrawOrder.MAP_BG2, true);
            DrawLayer(bounds, mapLayers[2], DrawOrder.MAP_FG, true);
        }

        public void Update()
        {
            foreach (var tile in _animatedTiles.Values)
            {
                tile.UpdateAnimation();
            }
        }

        public void ReloadTexture()
        {
            _tiles.Reload(mapName);
        }

        public Touching GetCollisionData(Vector2 pos)
        {
            Point p = ToMapLoc(pos);
            Touching ret = _tileObjects[mapLayers[0].GetTile(p)].allowCollisions;
            if(mapLayers[1].GetTile(p) != 0)
            {
                ret |= _tileObjects[mapLayers[1].GetTile(p)].allowCollisions;
            }
            return ret;
        }

        public int GetTile(Layer layer, Point pos)
        {
            return mapLayers[(int)layer].GetTile(pos);
        }

        public void ChangeTile(Layer layer, Point pos, int newVal)
        {
            mapLayers[(int)layer].ChangeTile(pos, newVal);
        }

        public Point ToMapLoc(Vector2 loc)
        {
            loc += offset;
            return (loc / GameConstants.TILE_WIDTH).ToPoint();
        }

        public Vector2 TileToWorld(Point p)
        {
            return p.ToVector2() * GameConstants.TILE_WIDTH - offset;
        }

        public SwapperControl.State CheckSwapper(Point tile)
        {
            //Not TileToWorld since swapper check needs to be in map coordinates
            return swapper.CheckCoord(tile.ToVector2()*GameConstants.TILE_WIDTH);
        }

        public void Collide(Entity e)
        {
            Rectangle hitbox = e.Hitbox;
            Point tl = ToMapLoc(new(hitbox.X, hitbox.Y));
            Point br = ToMapLoc(new(hitbox.Right, hitbox.Bottom));
            for (int y = tl.Y; y <= br.Y; ++y)
            {
                for (int x = tl.X; x <= br.X; ++x)
                {
                    Point pos = new(x, y);
                    CollideTile(pos, _tileObjects[mapLayers[0].GetTile(pos)], e);
                    int bg2 = mapLayers[1].GetTile(pos);
                    if(bg2 != 0)
                    {
                        //tile 0 never has collision in bg2(important for maps in which 0 does have collision in map, but is used as transparent in bg2)
                        CollideTile(pos, _tileObjects[bg2], e);
                    }
                }
            }
        }

        public void ReloadSettings(Vector2 player_pos)
        {
            if (settings is null)
                return;
#nullable enable
            Sounds.SoundManager.PlaySong(settings.Get(s => s.Music, player_pos, ""));
            GlobalState.darkness.TargetAlpha(settings.Get(s => s.DarknessAlpha, player_pos, 0f));
#nullable restore
        }

        public void OnTransitionStart()
        {
            foreach(TileMap m in mapLayers)
            {
                m.OnTransitionStart();
            }
        }

        public void OnTransitionEnd()
        {
            foreach (TileMap m in mapLayers)
            {
                m.OnTransitionEnd();
            }
        }

        private void DrawLayer(Rectangle bounds, TileMap map, Drawing.DrawOrder layer, bool ignoreEmpty = false)
        {
            float z = DrawingUtilities.GetDrawingZ(layer,0);

            Point tl = ToMapLoc(new(bounds.X, bounds.Y));
            Point br = ToMapLoc(new(bounds.Right, bounds.Bottom));
            for (int y = tl.Y - 1; y < br.Y + 1; y++)
            {
                for (int x = tl.X - 1; x < br.X + 1; x++)
                {
                    int tile = map.GetTile(new(x, y));

                    if (_tileObjects[tile].visible)
                    {
                        if (!ignoreEmpty || tile != 0)
                        {
                            Texture2D tex = _tiles.Tex;
                            Rectangle source = _tiles.GetRect(tile);

                            if (layer == DrawOrder.MAP_BG && _animatedTiles.TryGetValue(tile, out AnimatedTile animTile))
                            {
                                tex = animTile.sprite.Tex;
                                source = animTile.spriteRect;
                            }
                            Vector2 loc = TileToWorld(new(x, y));
                            SpriteDrawer.DrawSprite(tex, new Rectangle((int)loc.X, (int)loc.Y, _tiles.Width, _tiles.Height), source, Z: z);
                        }
                    }
                }
            }
        }

        private void CollideTile(Point tilePos, Tile t, Entity ent)
        {
            t.lastPosition = t.Position = TileToWorld(tilePos);

            if (t.allowCollisions == Touching.NONE || GameObject.Separate(ent, t))
            {
                if (t.collisionEventType != CollisionEventType.NONE && ent.MapInteraction)
                {
                    switch (t.collisionEventType)
                    {
                        case CollisionEventType.CONVEYOR:
                            if (t.Hitbox.Contains(ent.Center))
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
                            if (t.Hitbox.Contains(ent.Center))
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
