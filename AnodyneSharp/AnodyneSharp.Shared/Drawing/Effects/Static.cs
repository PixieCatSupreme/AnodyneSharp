using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AnodyneSharp.Drawing.Effects
{
    public class Static : IFullScreenEffect
    {
        private Effect effect;
        private float static_timer = 0;
        private int static_step = 0;
        public bool active = false;

        public bool Active()
        {
            return active;
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            effect = content.Load<Effect>("effects/static");
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            effect.Parameters["Projection"].SetValue(SpriteDrawer.Projection(screen.Bounds.Size));
            batch.Begin(effect: effect);
            batch.Draw(screen, screen.Bounds, Color.White);
            batch.End();
        }

        public void Update()
        {
            static_timer += GameTimes.DeltaTime;
            if (static_timer > 1.0f / 8.0f)
            {
                static_timer = 0;
                static_step = (static_step + 1) % 4;
                effect.Parameters["step"].SetValue(static_step);
            }
        }

        public void Deactivate()
        {
            active = false;
        }
    }
}
