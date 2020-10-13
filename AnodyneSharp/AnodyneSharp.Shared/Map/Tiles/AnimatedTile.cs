using AnodyneSharp.Drawing.Spritesheet;
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
        public Spritesheet sprite;

        public Rectangle spriteRect;

        public AnimatedTile(int[] frames, int framerate, Texture2D texture)
        {
            _curAnim = new Anim("a", frames, framerate);

            sprite = new Spritesheet(texture, 16, 16);
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
            spriteRect = sprite.GetRect(_curAnim.Frame);
        }
    }
}
