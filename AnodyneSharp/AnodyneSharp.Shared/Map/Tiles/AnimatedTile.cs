using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Map.Tiles
{
    public class AnimatedTile
    {
        private Anim _curAnim;
        public Texture2D Texture;

        public Rectangle spriteRect;

        public AnimatedTile(int[] frames, int framerate, Texture2D texture)
        {
            _curAnim = new Anim("a", frames, framerate);

            Texture = texture;
        }

        public void UpdateAnimation()
        {
            _curAnim.Update();

            if (_curAnim.Dirty)
            {
                UpdateRect();
            }
        }

        private void UpdateRect()
        {
            int indexX = _curAnim.Frame * 16;
            int indexY = 0;

            //Handle sprite sheets
            int texWidth = Texture.Bounds.Width;
            if (indexX >= texWidth)
            {
                indexY = indexX / texWidth * 16;
                indexX %= texWidth;
            }

            spriteRect = new Rectangle(indexX, indexY, 16, 16);
        }
    }
}
