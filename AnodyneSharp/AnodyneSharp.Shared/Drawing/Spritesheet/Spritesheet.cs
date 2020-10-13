using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Spritesheet
{
    public class Spritesheet
    {
        public readonly Texture2D Tex;
        public readonly int Width;
        public readonly int Height;

        public Spritesheet(Texture2D tex, int width, int height)
        {
            Tex = tex;
            Width = width;
            Height = height;
        }

        public Rectangle GetRect(int frame)
        {
            int indexX = frame * Width;
            int indexY = 0;

            if (indexX >= Tex.Width)
            {
                indexY = indexX / Tex.Width * Height;
                indexX %= Tex.Width;
            }

            return new Rectangle(indexX, indexY, Width, Height);
        }

    }
}
