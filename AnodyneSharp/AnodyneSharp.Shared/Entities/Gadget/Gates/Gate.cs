using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Gates
{
    [Collision(typeof(Player))]
    public abstract class Gate : Entity
    {
        public Gate(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer) : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        public abstract bool TryUnlock();
    }
}
