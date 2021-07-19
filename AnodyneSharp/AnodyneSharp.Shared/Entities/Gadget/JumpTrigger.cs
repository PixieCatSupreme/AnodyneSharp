using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("Jump_Trigger", ""), Collision(typeof(Player))]
    public class JumpTrigger : Entity
    {
        private int distance;
        private float time;

        public JumpTrigger(EntityPreset preset, Player p)
            : base(preset.Position, DrawOrder.ENTITIES)
        {
            visible = false;
            immovable = true;
            Solid = false;

            width = 6;
            height = 4;

            Position.X += 5;
            Position.Y += 12;

            switch (preset.Frame)
            {
                case 1:
                    distance = 48;
                    time = 0.5f;
                    break;
                case 2:
                    distance = 64;
                    time = 0.7f;
                    break;
                default:
                    distance = 32;
                    time = 0.3f;
                    break;
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Player p && (p.state == PlayerState.LADDER || (p.state == PlayerState.GROUND && p.facing == Facing.DOWN)))
            {
                p.AutoJump(time, p.Position + new Vector2(0, distance));
            }
        }
    }
}
