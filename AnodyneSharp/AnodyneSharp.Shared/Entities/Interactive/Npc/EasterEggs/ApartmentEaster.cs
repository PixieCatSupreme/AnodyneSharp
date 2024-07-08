using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.EasterEggs
{
    [NamedEntity("NPC", type: "generic", map: "APARTMENT", 8), Collision(typeof(Player))]
    public class ApartmentEaster : Entity, Interactable
    {
        public ApartmentEaster(EntityPreset preset, Player p)
            : base(preset.Position, "randoms", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
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
