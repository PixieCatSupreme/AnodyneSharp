using AnodyneSharp.Entities.Gadget.Doors;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("Fisherman"), Collision(typeof(Player), typeof(Broom), typeof(WhirlPool))]
    public class Fisherman : Entity
    {
        private Player player;
        private EntityPreset _preset;

        private int health;

        private bool dead;

        public Fisherman(EntityPreset preset, Player p) : base(preset.Position, "beach_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            player = p;

            AddAnimation("idle", CreateAnimFrameArray(10, 11), 3, true);
            AddAnimation("dead", CreateAnimFrameArray(12), 3, true);

            Play("idle");
            immovable = true;
            _preset = preset;

            health = 2;
        }

        public override void Update()
        {
            base.Update();

            if (dead)
            {
                MathUtilities.MoveTo(ref y_push, 16, 16 / 0.27f);
                MathUtilities.MoveTo(ref offset.Y, 14, 16 / 0.27f);
                MathUtilities.MoveTo(ref offset.X, -4, 16 / 0.27f);
                MathUtilities.MoveTo(ref scale, 0.70f, 0.40f / 0.27f);

                if (y_push == 16)
                {
                    exists = false;
                }
            }
        }

        public override void Collided(Entity other)
        {
            if (other is Broom b && health > 0 && !_flickering)
            {
                Flicker(0.5f);
                health--;

                if (health <= 0)
                {
                    velocity.Y = 17f;
                    Play("dead");

                    _preset.Alive = false;

                    player.state = PlayerState.INTERACT;
                }
            }
            else if (other is WhirlPool f && !dead && Position.Y >= _preset.Position.Y + 28)
            {
                dead = true;

                velocity = Vector2.Zero;
                f.DoTransition();

                player.state = PlayerState.GROUND;
            }
            else
            {
                Separate(this, other);
            }
        }
    }
}
