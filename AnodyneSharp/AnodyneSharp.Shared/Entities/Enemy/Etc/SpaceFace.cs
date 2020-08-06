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

        public SpaceFace(EntityPreset preset, Player p)
            : base(preset.Position, "space_npcs", 16, 16, Drawing.DrawOrder.BG_ENTITIES)
        {
            int[] anim;
            if (preset.Frame == 0)
            {
                anim = CreateAnimFrameArray(22, 23);
            }
            else
            {
                anim = CreateAnimFrameArray(20, 21);
            }

            AddAnimation("a", anim, 6);
            Play("a");

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
