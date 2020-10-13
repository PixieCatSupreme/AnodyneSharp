using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI
{
    public class UIEntity : Entity
    {
        public UIEntity(Vector2 pos, DrawOrder layer) 
            : base(pos, layer)
        { }

        public UIEntity(Vector2 pos, int frameWidth, int frameHeight, DrawOrder layer) 
            : base(pos, frameWidth, frameHeight, layer)
        { }

        public UIEntity(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer) 
            : base(pos, textureName, frameWidth, frameHeight, layer)
        { }

        public UIEntity(Vector2 pos, string textureName, int frame, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            SetFrame(frame);
        }

        public override void Draw()
        {
            if (visible)
            {
                SpriteDrawer.DrawGuiSprite(sprite.Tex, Position - offset, sprite.GetRect(_curAnim.Frame), Z: DrawingUtilities.GetDrawingZ(layer));
            }
        }

        protected override bool SetTexture(string textureName, int width, int height, bool ignoreChaos = false)
        {
            return base.SetTexture(textureName, width, height, true);
        }

        public override void ReloadTexture(bool ignoreChaos = false)
        {
            base.ReloadTexture(true);
        }
    }
}
