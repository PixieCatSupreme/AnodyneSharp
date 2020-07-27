using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget.Gates
{
    public class SmallKeyGate : KeyCardGate
    {
        public SmallKeyGate(Vector2 pos) : base(pos, "keyhole", 16, 16, DrawOrder.ENTITIES)
        {
            SetFrame(0);
            AddAnimation("Open", CreateAnimFrameArray(16, 17, 18, 19, 20), 10, false);
        }

        public override bool TryUnlock()
        {
            if (InventoryManager.GetCurrentMapKeys() > 0)
            {
                InventoryManager.RemoveCurrentMapKey();
                SoundManager.PlaySoundEffect("unlock");
                Solid = false;
                return true;
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("misc", "any", "keyblock", 0);
                return false;
            }
        }
    }
}
