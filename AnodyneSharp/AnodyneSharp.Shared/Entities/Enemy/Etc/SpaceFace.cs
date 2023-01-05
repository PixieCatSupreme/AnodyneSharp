using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Etc
{
    [NamedEntity("Space_Face"), Collision(typeof(Player))]
    public class SpaceFace : Entity
    {
        float waitTimer;
        float waitTimerMax;

        Player _player;

        public static AnimatedSpriteRenderer GetSprite(int frame)
        {
            int[] anim;
            if (frame == 0)
            {
                anim = new int[] { 22, 23 };
            }
            else
            {
                anim = new int[] { 20, 21 };
            }

            return new AnimatedSpriteRenderer("space_npcs", 16, 16, new Anim("a", anim, 6));
        }

        public SpaceFace(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(preset.Frame), Drawing.DrawOrder.BG_ENTITIES)
        {
            waitTimer = 0f;
            waitTimerMax = (float)(0.5 + 1.5 * GlobalState.RNG.NextDouble());

            _player = p;
        }


        public override void Update()
        {
            base.Update();


            waitTimer += GameTimes.DeltaTime;
            if (waitTimer >= waitTimerMax)
            {
                waitTimer = 0;
                MoveTowards(_player.Position, 55);
                drag = new Vector2(GlobalState.RNG.Next(15,26));
            }
        }

        public override void Collided(Entity other)
        {
            //TODO: Kaizo mode fun
            //if (other is Player p)
            //{
            //    p.ReceiveDamage(1);
            //}
        }
    }
}
