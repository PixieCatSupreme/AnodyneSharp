using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Holes
{
    [NamedEntity, Collision(typeof(Player))]
    public class Hole : Entity
    {
        public Hole(EntityPreset preset, Player p)
            : this(preset.Position, preset.Frame)
        { }

        public Hole(Vector2 position, int frame)
            : base(position, new StaticSpriteRenderer("hole", 16, 16, frame), DrawOrder.MAP_BG2)
        {
            immovable = true;
            Solid = false;
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
