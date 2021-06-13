using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RunningTradeNPCs
{
    [NamedEntity("Trade_NPC", null, 2), Collision(typeof(Player))]
    public class OutsideMonster : Entity, Interactable
    {
        public OutsideMonster(EntityPreset preset, Player p) : base(preset.Position, "fields_npcs",16,16,Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("walk_d", CreateAnimFrameArray(20, 21), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(22, 23), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(24, 25), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(26, 27), 4);
            Play("walk_d");
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("goldman", "outside");
            return true;
        }
    }
}
