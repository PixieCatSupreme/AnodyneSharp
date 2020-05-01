using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu
{
    public class MenuSelector : Entity
    {
        public MenuSelector(Vector2 pos) 
            : base(pos,61,15, DrawOrder.PAUSE_SELECTOR)
        {
            AddAnimation("flash", CreateAnimFrameArray(0, 1), 4, true);
            AddAnimation("inactive", CreateAnimFrameArray(3), 0, false);

            Texture = ResourceManager.GetTexture("menu_select_active");

            Play("flash");
        }

        public override void Draw()
        {
            SpriteDrawer.DrawGuiSprite(Texture, Position - offset, spriteRect, Z: DrawingUtilities.GetDrawingZ(layer));
        }
    }
}
