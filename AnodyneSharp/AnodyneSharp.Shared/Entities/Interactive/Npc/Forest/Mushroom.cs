using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    [NamedEntity("Forest_NPC", null, 20), Collision(typeof(Player), typeof(Broom))]
    class Mushroom : Entity
    {
        public Mushroom(EntityPreset preset, Player p)
            : base(preset.Position, "forest_npcs", 16, 16, DrawOrder.ENTITIES)
        {
            AddAnimation("idle", CreateAnimFrameArray(20), 4);
            AddAnimation("move", CreateAnimFrameArray(20, 21, 20, 22, 20, 21, 20, 22, 20), 8, false);
            Play("idle");

            immovable = true;
        }

        public override void Update()
        {
            base.Update();

            if (CurAnimName == "move" && _curAnim.Finished)
            {
                Play("idle");
            }
        }


        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Broom)
            {
                if (CurAnimName != "move")
                {
                    Play("move");

                    SoundManager.PlaySoundEffect("cross2", "cross3", "cross4");
                }
            }
            else
            {
                Separate(this, other);
            }
        }
    }
}
