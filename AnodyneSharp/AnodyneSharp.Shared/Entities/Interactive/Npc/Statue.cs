using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("NPC","statue"), Collision(typeof(Player))]
    public class Statue : Entity, Interactable
    {
        public Statue(EntityPreset preset, Player p)
            : base(preset.Position, "sage_statue", 16, 16, DrawOrder.ENTITIES)
        {
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            Separate(other, this);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("statue", "one");
            return true;
        }
    }
}
