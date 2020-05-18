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
    public abstract class BaseTreasure : Entity
    {
        protected int _dialogueID;

        public BaseTreasure(string textureName, Vector2 pos, int frameWidth, int frameHeight, int frame, int dialogueID = -1)
            : base(pos, textureName, frameWidth, frameHeight, DrawOrder.FG_SPRITES)
        {
            _dialogueID = dialogueID;

            SetFrame(frame);
        }

        public BaseTreasure(string textureName, Vector2 pos, int frame, int dialogueID = -1)
            : this(textureName, pos, 16, 16, frame, dialogueID)
        { }

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
    }
}
