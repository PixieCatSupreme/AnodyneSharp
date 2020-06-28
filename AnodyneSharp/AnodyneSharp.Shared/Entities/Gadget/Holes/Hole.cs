using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Holes
{
    [NamedEntity, Collision(typeof(Player))]
    public class Hole : Entity
    {
        public Hole(EntityPreset preset)
            : this(preset.Position, preset.Frame)
        { }

        public Hole(Vector2 position, int frame)
            : base(position, "hole", 16, 16, DrawOrder.MAP_BG2)
        {
            immovable = true;
            solid = false;

            SetFrame(frame);
        }

        public override void Collided(Entity other)
        {
            if (exists && other is Player player && player.state != PlayerState.AIR)
            {
                player.Fall(Position);
            }
        }
    }
}
