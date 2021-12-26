using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Blue
{
    [NamedEntity("Happy_NPC", null, 18), Collision(typeof(Player))]
    class Dam : Entity
    {
        private EntityPreset _preset;

        public Dam(EntityPreset preset, Player p)
            : base(preset.Position, GlobalState.CURRENT_MAP_NAME == "BLUE" ? "blue_npcs" : "happy_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            if (GlobalState.CURRENT_MAP_NAME == "BLUE")
            {
                AddAnimation("fall", CreateAnimFrameArray(10, 11, 12, 13, 14), 7, false);
                SetFrame(10);
            }
            else
            {
                AddAnimation("fall", CreateAnimFrameArray(18, 19, 20, 21, 22), 7, false);
                SetFrame(18);
            }

            immovable = true;

            _preset = preset;
        }

        public override void Update()
        {
            base.Update();

            if (!_preset.Alive)
            {
                return;
            }

            if (GlobalState.CURRENT_MAP_NAME == "BLUE")
            {
                if (GlobalState.PUZZLES_SOLVED >= 3)
                {
                    Play("fall");
                    _preset.Alive = false;
                }
            }
            else
            {
                if (GlobalState.PUZZLES_SOLVED >= 1)
                {
                    Play("fall");
                    _preset.Alive = false;
                }
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }
}
