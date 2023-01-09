using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
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

        public static AnimatedSpriteRenderer GetSprite(int o) => new(o == 8 ? "blue_npcs" : "happy_npcs", 16, 16,
            new Anim("idle", new int[] { 10 + o },1),
            new Anim("fall", new int[] { 10 + o, 11 + o, 12 + o, 13 + o, 14 + o },7,false)
            );

        public Dam(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(GlobalState.CURRENT_MAP_NAME == "BLUE" ? 8 : 0), Drawing.DrawOrder.ENTITIES)
        {
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
