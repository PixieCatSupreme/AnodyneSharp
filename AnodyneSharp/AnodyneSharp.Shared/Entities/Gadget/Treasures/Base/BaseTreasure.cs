using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public abstract class BaseTreasure : Entity
    {
        protected int _dialogueID;

        public BaseTreasure(string textureName, Vector2 pos, int frameWidth, int frameHeight, int frame, int dialogueID = -1)
            : base(pos, new StaticSpriteRenderer(textureName, frameWidth, frameHeight, frame), DrawOrder.FG_SPRITES)
        {
            _dialogueID = dialogueID;
        }

        public BaseTreasure(string textureName, Vector2 pos, int frame, int dialogueID = -1)
            : this(textureName, pos, 16, 16, frame, dialogueID)
        { }

        public virtual void GetTreasure()
        {
            SoundManager.PlaySoundEffect("gettreasure");

            if (_dialogueID != -1)
            {
                if (_dialogueID != -2)
                {
                    GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("misc", "any", "treasure", _dialogueID);
                }
                else
                {
                    GlobalState.Dialogue = $"This person broke everything.";
                }
            }
        }
    }
}
