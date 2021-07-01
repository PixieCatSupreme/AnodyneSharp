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

        public Icky(EntityPreset preset, Player p) : base(preset.Position, "fields_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            if(GlobalState.events.GetEvent("icky.rescued") == 0)
            {
                exists = false;
                return;
            }
            AddAnimation("walk_d", CreateAnimFrameArray(10, 11), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(12, 13), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(14, 15), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(16, 17), 4);
            Play("walk_d");
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
