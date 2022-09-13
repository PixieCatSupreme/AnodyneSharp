using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public UIEntity(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer, SpriteEffects flip) 
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            _flip = flip;
        }

        public UIEntity(Vector2 pos, string textureName, int frame, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            SetFrame(frame);
        }

        public override void Draw()
        {
            if (visible)
            {
                Rectangle srect = sprite.GetRect(_curAnim.Frame);

                SpriteDrawer.DrawGuiSprite(sprite.Tex, 
                    MathUtilities.CreateRectangle(Position.X - offset.X * scale, Position.Y - offset.Y * scale + (int)y_push, srect.Width * scale, srect.Height * scale),
                    srect, 
                    color * opacity, 
                    rotation, 
                    _flip,
                    Z: DrawingUtilities.GetDrawingZ(layer));
            }
        }

        protected override bool SetTexture(string textureName, int width, int height, bool ignoreChaos = false, bool allowFailure = true)
        {
            return base.SetTexture(textureName, width, height, true, allowFailure);
        }

        public override void ReloadTexture(bool ignoreChaos = false)
        {
            base.ReloadTexture(true);
        }
    }
}
