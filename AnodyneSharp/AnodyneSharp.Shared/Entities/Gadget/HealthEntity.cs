using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Entities.Interactive;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("Health_Dropper")]
    public class HealthEntity : HealthPickup
    {
        private EntityPreset _preset;
        public HealthEntity(EntityPreset preset, Player p)
            : base(preset.Position, preset.Frame == 0)
        {
            _preset = preset;

            exists = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (!exists)
            {
                _preset.Alive = false;
            }
        }
    }
}
