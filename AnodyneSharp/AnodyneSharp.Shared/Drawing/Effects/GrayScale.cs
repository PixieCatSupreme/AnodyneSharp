using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Drawing.Effects
{
    public class GrayScale : IFullScreenEffect
    {
        private Effect effect;

        public bool Active()
        {
            return GlobalState.CURRENT_MAP_NAME == "SUBURB";
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            effect = content.Load<Effect>("effects/blackwhite");
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            batch.Begin(effect: effect);
            batch.Draw(screen, new Rectangle(0, 0, screen.Width, screen.Height), Color.White);
            batch.End();
        }
    }
}
