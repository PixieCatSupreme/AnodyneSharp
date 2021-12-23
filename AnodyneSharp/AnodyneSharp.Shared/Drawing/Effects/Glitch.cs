using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class Glitch : IFullScreenEffect
    {
        List<GlitchRect> glitches = Enumerable.Repeat(0, 50).Select((c) => new GlitchRect()).ToList();
        float timer = 0f;
        float activation_time = 1 / 3f;
        public bool active = false;

        public bool Active()
        {
            return active;
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        { }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            batch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
            batch.Draw(screen, screen.Bounds, Color.White);
            foreach(GlitchRect rect in glitches) {
                rect.Draw(batch, screen);
            }
            batch.End();
        }

        public void Update()
        {
            timer += GameTimes.DeltaTime;
            if(timer >= activation_time)
            {
                timer -= activation_time;
                foreach (GlitchRect glitch in glitches)
                {
                    glitch.Recalc();
                }
            }
        }

        public void Deactivate()
        { }

        class GlitchRect
        {
            Rectangle source;
            Point dest;

            public GlitchRect()
            {
                Recalc();
            }

            public void Recalc()
            {
                source = new Rectangle(GlobalState.RNG.Next(160), GlobalState.RNG.Next(180), GlobalState.RNG.Next(16, 32), GlobalState.RNG.Next(16, 32));
                dest = new Point(GlobalState.RNG.Next(160),GlobalState.RNG.Next(180));
            }


            public void Draw(SpriteBatch batch, Texture2D screen)
            {
                batch.Draw(screen, new Rectangle(dest, source.Size), source, Color.White);
            }

        }

    }
}
