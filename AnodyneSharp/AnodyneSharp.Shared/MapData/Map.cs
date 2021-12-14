using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Entities;
using AnodyneSharp.MapData.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.MapData
{
    public class Map
    {
        public enum Layer
        {
            BG,
            BG2,
            FG
        }

        MapLayer[] layers;

        public int WidthInTiles => layers[0].WidthInTiles;
        public int HeightInTiles => layers[0].HeightInTiles;

        public Map(string name)
        {
            layers = new MapLayer[] { new(), new(), new() };
            Spritesheet tiles = TileData.GetTileset(name);
            layers[0].LoadMap(MapLoader.GetMapLayer(name), tiles, Drawing.DrawOrder.MAP_BG);
            layers[1].LoadMap(MapLoader.GetMapLayer(name, 2), tiles, Drawing.DrawOrder.MAP_BG2);
            layers[2].LoadMap(MapLoader.GetMapLayer(name, 3), tiles, Drawing.DrawOrder.MAP_FG);
            TileData.SetTileProperties(name, layers[0], layers[1]);
        }

        public MapLayer GetLayer(Layer layer)
        {
            return layers[(int)layer];
        }

        public void Draw(Rectangle bounds)
        {
            layers[0].Draw(bounds);
            layers[1].Draw(bounds, true);
            layers[2].Draw(bounds, true);
        }

        public void Update()
        {
            layers[0].Update(); //update anims
        }

        public void ReloadTexture()
        {
            layers[0].ReloadTexture();
        }

        public Touching GetCollisionData(Vector2 pos)
        {
            return layers[0].GetCollisionData(pos) | layers[1].GetCollisionData(pos);
        }

        public int GetTile(Layer layer, Point pos)
        {
            return GetLayer(layer).Data.GetTile(pos);
        }

        public void ChangeTile(Layer layer, Point pos, int newVal)
        {
            GetLayer(layer).Data.ChangeTile(pos, newVal);
        }

        public void Collide(Entity e)
        {
            layers[0].Collide(e);
            layers[1].Collide(e);
        }

        public void OnTransitionStart()
        {
            foreach(MapLayer l in layers)
            {
                l.Data.OnTransitionStart();
            }
        }

        public void OnTransitionEnd()
        {
            foreach(MapLayer l in layers)
            {
                l.Data.OnTransitionEnd();
            }
        }

        public Vector2 GetFirstWalkable()
        {
            return layers[0].GetFirstWalkable(layers[1]);
        }
    }
}
