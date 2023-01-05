using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.EasterEggs
{
    [NamedEntity("NPC", type: "generic", map: "REDCAVE", 8), Collision(typeof(Player))]
    class RedCaveEaster : Entity, Interactable
    {
        public RedCaveEaster(EntityPreset preset, Player p)
            : base(preset.Position, "smoke_red", 32, 32, Drawing.DrawOrder.ENTITIES)
        {
            SetFrame(0);

            immovable = true;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "easter_egg");

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }
}
