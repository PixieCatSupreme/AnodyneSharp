using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Map
{
    public class Minimap
    {
        int[] interest;
        public readonly TileMap tiles;

        private int CurrentLoc => GlobalState.CURRENT_GRID_X + GlobalState.CURRENT_GRID_Y * tiles.Width;

        private const int PlayerIndicator = 27;
        private const int ChestIndicator = 18;

        public Minimap(TileMap map)
        {
            tiles = map;
            interest = new int[map.Width * map.Height];
        }

        public void Update()
        {
            int x = GlobalState.CURRENT_GRID_X;
            int y = GlobalState.CURRENT_GRID_Y;
            if (x >= 0 && x < tiles.Width && y >= 0 && y < tiles.Height)
            {
                interest[CurrentLoc] = 1;
            }
        }

        //Points-of-interest(only chests for now) get added to the minimap and removed if opened.
        public void AddInterest()
        {
            if(interest.Length > 0)
                interest[CurrentLoc] += 1;
        }

        public void RemoveInterest()
        {
            if(interest.Length > 0)
                interest[CurrentLoc] -= 1;
        }

        /// <param name="sprites">Spritesheet to use when drawing</param>
        /// <param name="topleft">Top left coordinate of the 0,0 tile</param>
        /// <param name="bounds">Part of the minimap to draw</param>
        public void Draw(Spritesheet sprites, Vector2 topleft, Rectangle? bounds = null, bool DrawPlayerIndicator = true)
        {
            Rectangle b = bounds ?? new Rectangle(0, 0, tiles.Width, tiles.Height);
            for (int y = b.Top; y < b.Bottom; ++y)
                for (int x = b.Left; x < b.Right; ++x)
                {
                    Vector2 pos = topleft + new Vector2(x * sprites.Width, y * sprites.Height);
                    int visible = interest[x + y * tiles.Width];
                    if (visible > 0)
                    {
                        SpriteDrawer.DrawGuiSprite(sprites.Tex, pos, sprites.GetRect(tiles.GetTile(x, y)), Z: DrawingUtilities.GetDrawingZ(DrawOrder.MINIMAP));
                    }
                    if (visible > 1)
                    {
                        SpriteDrawer.DrawGuiSprite(sprites.Tex, pos, sprites.GetRect(ChestIndicator), Z: DrawingUtilities.GetDrawingZ(DrawOrder.MINIMAP_CHEST));
                    }

                }

            Vector2 PlayerPos = new Vector2(GlobalState.CURRENT_GRID_X, GlobalState.CURRENT_GRID_Y);
            if (b.Contains(PlayerPos) && DrawPlayerIndicator)
            {
                SpriteDrawer.DrawGuiSprite(sprites.Tex, topleft + PlayerPos * new Vector2(sprites.Width), sprites.GetRect(PlayerIndicator), Z: DrawingUtilities.GetDrawingZ(DrawOrder.MINIMAP_PLAYER));
            }
        }
    }
}
