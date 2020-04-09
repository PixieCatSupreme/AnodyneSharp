using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Utilities
{
    public static class TextureUtilities
    {
        public static Texture2D LoadTexture(string folder, string file, ContentManager content)
        {
            return content.Load<Texture2D>($"{folder}/{file}");
        }
    }
}
