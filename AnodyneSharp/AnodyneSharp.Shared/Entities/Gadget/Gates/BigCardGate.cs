using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("CardGate")]
    public class BigCardGate : BigGate
    {
        public BigCardGate(EntityPreset preset, Player p) : base(preset,p)
        {
            immovable = true;
            _sentinel.OpensOnInteract = true;

            int frame = preset.Frame switch
            {
                4 => 8,
                8 => 9,
                16 => 10,
                47 => 11,
                48 => 12,
                24 => 13,
                36 => 14,
                49 => 15,
                50 => 16,
                _ => 0
            };
            SetFrame(frame);
        }

        public override bool TryUnlock()
        {
            if (InventoryManager.CardCount >= _preset.Frame)
            {
                GlobalState.Dialogue = _preset.Frame switch
                {
                    4 => DialogueManager.GetDialogue("misc", "any", "keyblockgate", 1),
                    < 36 => DialogueManager.GetDialogue("misc","any","keyblockgate",4),
                    36 => DialogueManager.GetDialogue("misc", "any", "keyblockgate", 3),
                    47 => DialogueManager.GetDialogue("misc", "any", "keyblockgate", 5),
                    49 => "!!!",
                    _ => null
                };
                _state.ChangeState("Open");
                return true;
            }
            else
            {
                GlobalState.Dialogue = _preset.Frame switch
                {
                    4 => DialogueManager.GetDialogue("misc", "any", "keyblockgate", 0),
                    49 => "....",
                    50 => DialogueManager.GetDialogue("misc", "any", "keyblockgate", 6),
                    _ => DialogueManager.GetDialogue("misc", "any", "keyblockgate", 2)
                };
                return false;
            }
        }
    }
}
