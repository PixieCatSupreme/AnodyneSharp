using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    [NamedEntity("Forest_NPC", null, 0), Collision(typeof(Player))]
    class Thorax : Entity, Interactable
    {
        private Player _player;

        public Thorax(EntityPreset preset, Player p)
            : base(preset.Position, "forest_npcs", 16, 16, DrawOrder.ENTITIES)
        {
            AddAnimation("move", CreateAnimFrameArray(0, 1), 4);
            AddAnimation("stand", CreateAnimFrameArray(1), 0);
            AddAnimation("squat", CreateAnimFrameArray(0), 0);
            Play("move");

            _player = p;

            immovable = true;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("forest_npc", "thorax");

            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            Separate(this, other);
        }
    }
}
