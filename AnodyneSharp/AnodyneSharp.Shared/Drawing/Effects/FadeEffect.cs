using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class FadeEffect : IFullScreenEffect
    {
        public Color fadeColor;
        public float alpha = 0.0f;

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            batch.GraphicsDevice.Clear(fadeColor);

            batch.Begin();
            batch.Draw(screen, screen.Bounds, Color.White * (1 - alpha));
            batch.End();
        }

        public void ForceAlpha(float alpha)
        {
            this.alpha = alpha;
        }

        public void ChangeAlpha(float speed)
        {
            alpha = Math.Clamp(alpha + speed * GameTimes.DeltaTime, 0f, 1f);
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        { }

        public bool Active()
        {
            return alpha != 0f;
        }

        public void Deactivate()
        {
            alpha = 0;
        }
    }
}
