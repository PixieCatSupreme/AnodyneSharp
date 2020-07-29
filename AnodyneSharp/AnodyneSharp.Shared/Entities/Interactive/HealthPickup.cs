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

        public HealthPickup(Vector2 position, bool isBigHealth)
            : base(position, Drawing.DrawOrder.ENTITIES)
        {
            if (isBigHealth)
            {
                SetTexture("big_health_pickup");
                width = frameWidth = 16;
                healingFactor = 3;
            }
            else
            {
                SetTexture("small_health_pickup");
                width = frameWidth = 10;
            }

            AddAnimation("float", CreateAnimFrameArray(0, 1, 2, 3), 5);
            height = frameHeight = 16;

            Play("float");

            exists = false;
        }

        public override void Collided(Entity other)
        {
            SoundManager.PlaySoundEffect("get_small_health");
            GlobalState.CUR_HEALTH += healingFactor;
            exists = false;

        }
    }
}
