using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public abstract class BaseTreasure
    {
        public bool visible;
        public bool finished;

        protected Rectangle _texRect;

        protected  int _dialogueID;
        private Texture2D _texture;
        private Rectangle _texSourceRect;

        public BaseTreasure(string textureName, Vector2 pos, int frame, int dialogueID = -1)
        {
            _dialogueID = dialogueID;

            _texRect = new Rectangle((int)pos.X, (int)pos.Y, 16,16);

            _texture = ResourceManager.GetTexture(textureName);
            UpdateRect(frame);
        }

        public virtual void GetTreasure()
        {
            SoundManager.PlaySoundEffect("gettreasure");

            if (_dialogueID != -1)
            {
                //TODO localization and such
                if (_dialogueID != -2)
                {
                    GlobalState.Dialogue = $"test treasure dialogue {_dialogueID}";
                }
                else
                {
                    GlobalState.Dialogue = $"This person broke everything.";
                }
            }
        }

        public abstract void Update();
        public virtual void Draw()
        {
            if (!finished && visible)
            {
                SpriteDrawer.DrawSprite(_texture, _texRect, _texSourceRect, Z: DrawingUtilities.GetDrawingZ(DrawOrder.FG_SPRITES));
            }
        }

        private void UpdateRect(int frame)
        {
            int indexX = frame * 16;
            int indexY = 0;

            //Handle sprite sheets
            int texWidth = _texture.Bounds.Width;
            if (indexX >= texWidth)
            {
                indexY = indexX / texWidth * 16;
                indexX %= texWidth;
            }

            _texSourceRect = new Rectangle(indexX, indexY, 16, 16);
        }
    }
}
