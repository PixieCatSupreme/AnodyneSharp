using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Hotel
{
    [NamedEntity("NPC", "generic", 12), Collision(typeof(Player))]
    class HotelGuy : Entity, Interactable
    {
        public HotelGuy(EntityPreset preset, Player p)
            : base(preset.Position, "hotel_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("idle_d", CreateAnimFrameArray(0), 8);
            AddAnimation("idle_r", CreateAnimFrameArray(1), 8);
            AddAnimation("idle_u", CreateAnimFrameArray(2), 8);
            AddAnimation("idle_l", CreateAnimFrameArray(3), 8);
            Play("idle_u");

            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            FaceTowards(Position - FacingDirection(player_direction));
            PlayFacing("idle");

            GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "one");

            return true;
        }
    }
}
