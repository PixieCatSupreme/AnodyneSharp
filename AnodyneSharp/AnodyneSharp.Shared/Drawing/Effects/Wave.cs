using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class Wave : IFullScreenEffect
    {
        Effect effect;
        float timer = 0;

        public bool active = false;

        public bool Active()
        {
            return active;
        }

        public void Update()
        {
            //Loops every two seconds
            timer += GameTimes.DeltaTime * 0.5f;
            if(timer > 1f)
            {
                timer -= 1f;
            }
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            effect = content.Load<Effect>("effects/wave");
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            effect.Parameters["phase_offset"].SetValue((int)(timer*180));
            effect.Parameters["Projection"].SetValue(SpriteDrawer.Projection(screen.Bounds.Size));
            batch.Begin(effect: effect);
            batch.Draw(screen, screen.Bounds, Color.White);
            batch.End();
        }

        public void Deactivate()
        {
            active = false;
        }
    }
}
