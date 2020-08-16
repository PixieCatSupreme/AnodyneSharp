using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AnodyneSharp.Drawing.Effects
{
    public class Pixelate : IFullScreenEffect
    {
        private Effect effect;
        private float pixelation = 1f;

        public void AddPixelation(float speed)
        {
            pixelation += speed * GameTimes.DeltaTime;
            pixelation = Math.Max(1.0f, pixelation);
        }

        public void SetPixelation(float p)
        {
            pixelation = p;
        }

        public bool Active()
        {
            return (int)pixelation > 1;
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            effect = content.Load<Effect>("effects/pixelation");
            effect.CurrentTechnique = effect.Techniques["Pixelate"];
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            effect.Parameters["ScreenSize"].SetValue(new Vector2(screen.Width, screen.Height));
            batch.Begin(effect: effect);
            batch.Draw(screen, screen.Bounds, Color.White);
            batch.End();
        }

        public void Update()
        {
            effect.Parameters["StrideSize"].SetValue((int)pixelation);
        }
    }
}

