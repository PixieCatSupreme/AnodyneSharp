using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Interactive;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities
{
    public class HealthDropper : Entity
    {
        private float _healthDropChance = 0.5f;

        private HealthPickup _health;

        private EntityPreset _preset;

        public HealthDropper(EntityPreset p, Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer, float healthDropChance = 0.5f, bool dropBigHealth = false) 
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
            _health = new HealthPickup(pos, dropBigHealth);

            _healthDropChance = healthDropChance;

            _preset = p;
        }

        public HealthDropper(EntityPreset p, Vector2 pos, AnimatedSpriteRenderer sprite, DrawOrder layer, float healthDropChance = 0.5f, bool dropBigHealth = false)
            : base(pos,sprite,layer)
        {
            _health = new HealthPickup(pos, dropBigHealth);

            _healthDropChance = healthDropChance;

            _preset = p;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(_health, 1);
        }

        protected virtual void Die()
        {
            if (_preset != null) _preset.Alive = false;
            exists = false;

            if (GlobalState.settings.guaranteed_health || GlobalState.RNG.NextDouble() < _healthDropChance)
            {
                _health.exists = true;
                _health.Position = Position;
            }
        }
    }
}
