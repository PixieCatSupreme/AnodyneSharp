using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [Collision(PartOfMap = true)]
    public abstract class KeyCardGate : Entity
    {
        public EntityPreset _preset;
        protected KeyBlockSentinel _sentinel;

        public KeyCardGate(EntityPreset preset, string textureName, int frameWidth, int frameHeight, DrawOrder layer) : base(preset.Position, textureName, frameWidth, frameHeight, layer)
        {
            _preset = preset;
            _sentinel = new(this);
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(_sentinel, 1);
        }

        public abstract bool TryUnlock();
    }
}
