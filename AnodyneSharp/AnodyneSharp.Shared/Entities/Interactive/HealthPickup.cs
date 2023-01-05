using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive
{
    [Collision(typeof(Player), typeof(Broom))]
    public class HealthPickup : Entity
    {
        private int healingFactor = 1;

        private float _latency = 0.5f;

        private static AnimatedSpriteRenderer GetSprite(bool isBigHealth)
        {
            int o = GlobalState.IsCell ? 4 : 0;
            Anim anim = new("float", new int[] { 0 + o, 1 + o, 2 + o, 3 + o }, 5);

            if (isBigHealth)
            {
                return new AnimatedSpriteRenderer("big_health_pickup", 16, 16, anim);

            }
            else
            {
                return new AnimatedSpriteRenderer("small_health_pickup", 10, 16, anim);
            }
        }

        public HealthPickup(Vector2 position, bool isBigHealth)
            : base(position, GetSprite(isBigHealth), Drawing.DrawOrder.ENTITIES)
        {
            if (isBigHealth)
            {
                healingFactor = 3;
            }

            height = sprite.Height;
            width = sprite.Width;

            Play("float");

            exists = false;
        }

        public override void Update()
        {
            base.Update();

            _latency -= GameTimes.DeltaTime;
        }

        public override void Collided(Entity other)
        {
            if (_latency <= 0)
            {
                SoundManager.PlaySoundEffect("get_small_health");
                GlobalState.CUR_HEALTH += healingFactor;
                exists = false;
            }
        }
    }
}
