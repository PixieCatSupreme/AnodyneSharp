using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RunningTradeNPCs
{
    [NamedEntity("Trade_NPC", null, 4), Collision(typeof(Player))]
    public class Icky : Entity, Interactable
    {
        Player _player;

        public static AnimatedSpriteRenderer GetSprite() => new("fields_npcs", 16, 16,
            new Anim("walk_d", new int[] { 10, 11 }, 4),
            new Anim("walk_r", new int[] { 12, 13 }, 4),
            new Anim("walk_u", new int[] { 14, 15 }, 4),
            new Anim("walk_l", new int[] { 16, 17 }, 4)
            );

        public Icky(EntityPreset preset, Player p) : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            if(GlobalState.events.GetEvent("icky.rescued") == 0)
            {
                exists = false;
                return;
            }
            immovable = true;
            _player = p;
        }

        public override void Update()
        {
            base.Update();
            FaceTowards(_player.Center);
            PlayFacing("walk");
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("miao", "icky");
            return true;
        }
    }
}
