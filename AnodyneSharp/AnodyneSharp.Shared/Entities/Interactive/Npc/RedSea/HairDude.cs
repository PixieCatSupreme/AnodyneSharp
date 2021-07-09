using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.RedSea
{
    [NamedEntity("Redsea_NPC", null, 0), Collision(typeof(Player))]
    class HairDude : Entity, Interactable
    {
        private Player _player;

        public HairDude(EntityPreset preset, Player p)
            : base(preset.Position, "redsea_npcs", 16, 16, DrawOrder.ENTITIES)
        {
            AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(6, 7), 4);
            Play("walk_d");

            _player = p;

            immovable = true;
        }

        public override void Update()
        {
            base.Update();

            FaceTowards(_player.Position);

            PlayFacing("walk");
        }

        public bool PlayerInteraction(Facing player_direction)
        {

            //TODO the play second stuff
            if (false)
            {
                //GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "second");
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "first");
            }

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            Separate(this, other);
        }
    }
}