using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("KeyBlock",null,0)]
    public class SmallKeyGate : KeyCardGate
    {
        public SmallKeyGate(EntityPreset preset, Player p) : base(preset, "keyhole", 16, 16, DrawOrder.ENTITIES)
        {
            SetFrame(0);
            AddAnimation("Open", CreateAnimFrameArray(16, 17, 18, 19, 20), 10, false);
        }

        public override bool TryUnlock()
        {
            if (GlobalState.inventory.GetCurrentMapKeys() > 0)
            {
                GlobalState.inventory.RemoveCurrentMapKey();
                SoundManager.PlaySoundEffect("unlock");
                Play("Open");
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
