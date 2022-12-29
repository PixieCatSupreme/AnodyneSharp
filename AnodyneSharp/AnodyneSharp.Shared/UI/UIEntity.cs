using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Base.Rendering;
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

        public UIEntity(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            HasVisibleHitbox = false;
        }

        public UIEntity(Vector2 pos, string textureName, int frameWidth, int frameHeight, ILayerType layer)
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            HasVisibleHitbox = false;
        }

        public UIEntity(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer, SpriteEffects flip) 
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            _flip = flip;
            HasVisibleHitbox = false;
        }

        public UIEntity(Vector2 pos, string textureName, int frame, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            SetFrame(frame);
            HasVisibleHitbox = false;
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
