using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Space
{
    [NamedEntity("NPC", "generic", 4), Collision(typeof(Player))]
    class CubeKings : Entity, Interactable
    {
        private string _dialogue;

        public CubeKings(EntityPreset preset, Player p)
            : base(preset.Position, "space_npcs", 32, 32, Drawing.DrawOrder.ENTITIES)
        {
            if (GlobalState.CURRENT_GRID_X < 5)
            {
                AddAnimation("a", CreateAnimFrameArray(10, 11), 4);
                _dialogue = "gray";
            }
            else
            {
                AddAnimation("a", CreateAnimFrameArray(12, 13), 4);
                _dialogue = "color";
            }

            Play("a");

            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("cube_king", _dialogue);

            return true;
        }
    }
}
