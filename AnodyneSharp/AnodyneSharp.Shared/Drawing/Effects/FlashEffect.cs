using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class FlashEffect : IFullScreenEffect
    {
        FadeEffect effect = new();
        private float time = 1f;

        public void Flash(float time, Color c)
        {
            effect.fadeColor = c;
            this.time = time;
            effect.alpha = 1.0f;
        }

        public void Update()
        {
            effect.ChangeAlpha(-1f / time);
        }

        public bool Active()
        {
            return effect.Active();
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            effect.Load(content, graphicsDevice);
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            effect.Render(batch, screen);
        }

        public void ForceAlpha(float a)
        {
            effect.alpha = a;
        }
    }
}
