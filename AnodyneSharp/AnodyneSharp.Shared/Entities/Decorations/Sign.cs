using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Decorations
{
    [NamedEntity("Solid_Sprite", "sign"), Collision(PartOfMap = true)]
    public class Sign : Entity, Interactable
    {
        public Sign(EntityPreset preset, Player p)
            : base(preset.Position, "note_rock", 16, 16, DrawOrder.ENTITIES)
        {
            immovable = true;

            SetFrame(preset.Frame);
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("misc", "any", "solidsprite", GetFrame() -2);
            return true;
        }
    }
}
