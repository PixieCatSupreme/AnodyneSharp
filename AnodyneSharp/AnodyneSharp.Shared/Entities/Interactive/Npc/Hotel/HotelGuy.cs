using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Hotel
{
    [NamedEntity("NPC", "generic", 12), Collision(typeof(Player))]
    public class HotelGuy : Entity, Interactable
    {
        public static AnimatedSpriteRenderer GetSprite() => new("hotel_npcs", 16, 16,
            new Anim("idle_d", new int[] { 0 },1),
            new Anim("idle_r", new int[] { 1 }, 1),
            new Anim("idle_u", new int[] { 2 }, 1),
            new Anim("idle_l", new int[] { 3 }, 1)
            );

        public HotelGuy(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
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
