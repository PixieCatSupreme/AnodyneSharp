using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Drawing.Effects
{
    public interface IFullScreenEffect
    {
        //Called on game load
        public void Load(ContentManager content, GraphicsDevice graphicsDevice);

        //Renders the screen to the batch with its effect applied
        public void Render(SpriteBatch batch, Texture2D screen);

        //Returns true if the effect needs to be applied
        public bool Active();

        public void Update() { }
    }
}
