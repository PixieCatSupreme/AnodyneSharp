using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Decorations
{
    [NamedEntity("Solid_Sprite","tree"), Collision(typeof(Player))]
    public class BigTree : Entity
    {
        public BigTree(EntityPreset preset, Player p) : base(preset.Position,"TREE",64,64,DrawOrder.ENTITIES)
        {
            offset = new(16, 32);
            width = height = 32;
            Position += offset;
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }
}
