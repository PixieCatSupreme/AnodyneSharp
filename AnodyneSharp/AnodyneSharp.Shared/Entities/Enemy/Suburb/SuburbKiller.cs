using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Suburb
{
    [NamedEntity("Suburb_Killer"), Collision(typeof(Player))]
    class SuburbKiller : Entity
    {
        private bool moving;
        private float moveTimer;

        private Player _player;


        public SuburbKiller(EntityPreset preset, Player p)
            : base(preset.Position, "suburb_killers", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            _player = p;

            int off = GlobalState.RNG.Next(0, 7) * 9;

            AddAnimation("idle_d", CreateAnimFrameArray(off + 0), 1);
            AddAnimation("idle_r", CreateAnimFrameArray(off + 2), 1);
            AddAnimation("idle_u", CreateAnimFrameArray(off + 4), 1);
            AddAnimation("idle_l", CreateAnimFrameArray(off + 6), 1);

            AddAnimation("walk_d", CreateAnimFrameArray(off, off + 1), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(off + 2, off + 3), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(off + 4, off + 5), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(off + 6, off + 7), 4);

            Play("idle_d");

            width = height = 6;
            offset = new Vector2(5);

            Position += new Vector2(5);
        }

        public override void Update()
        {
            base.Update();

            FaceTowards(_player.Position);

            if (!moving)
            {
                PlayFacing("idle");

                moving = (Position - _player.Position).Length() < 36;
            }
            else
            {
                PlayFacing("walk");

                moveTimer += GameTimes.DeltaTime;

                if (moveTimer > 0.5f)
                {
                    moveTimer = 0;

                    MoveTowards(_player.Position, 30);
                }

            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Player p)
            {
                p.ReceiveDamage(6);
            }
        }
    }
}
