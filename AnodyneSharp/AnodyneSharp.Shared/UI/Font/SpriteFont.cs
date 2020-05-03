using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.Font
{
    public class SpriteFont
    {
        public readonly int lineHeight;
        public readonly int spaceWidth;
        public Texture2D texture;
        public Color color;

        public int lineSeparation
        {
            get
            {
                return lineHeight + 2;
            }
        }

        private Dictionary<char, Rectangle> _characterRectangles;

        public SpriteFont(int lineHeight, int spaceWidth, string textureName, string characterOrder, Color color)
        {
            this.lineHeight = lineHeight;
            this.spaceWidth = spaceWidth;

            texture = ResourceManager.GetTexture(textureName, true);
            _characterRectangles = CreateRectangles(characterOrder);

            this.color = color;
        }
        public Rectangle? GetRectangle(char character)
        {
            if (_characterRectangles.ContainsKey(character))
            {
                return _characterRectangles[character];

            }
            else
            {
                return null;
            }
        }

        private Dictionary<char, Rectangle> CreateRectangles(string characterOrder)
        {
            var d = new Dictionary<char, Rectangle>();
            int texWidth = texture.Bounds.Width;

            for (int i = 0; i < characterOrder.Length; i++)
            {
                char c = characterOrder[i];

                int indexX = i * spaceWidth;
                int indexY = 0;

                if (indexX >= texWidth)
                {
                    indexY = indexX / texWidth * lineHeight;
                    indexX %= texWidth;
                }

                d.Add(characterOrder[i], new Rectangle(indexX, indexY, spaceWidth,lineHeight));
            }

            return d;
        }
    }
}