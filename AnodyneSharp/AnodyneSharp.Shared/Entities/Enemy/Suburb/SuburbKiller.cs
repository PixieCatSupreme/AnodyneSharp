using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Suburb
{
    [NamedEntity("Suburb_Killer"), Collision(typeof(Player))]
    public class SuburbKiller : Entity
    {
        private bool moving;
        private float moveTimer;

        public const string DamageDealer = "Suburb killer";

        private Player _player;

        public static AnimatedSpriteRenderer GetSprite(int off) => new("suburb_killers", 16, 16,
            new Anim("idle_d", new int[] { off + 0 }, 1),
            new Anim("idle_r", new int[] { off + 2 }, 1),
            new Anim("idle_u", new int[] { off + 4 }, 1),
            new Anim("idle_l", new int[] { off + 6 }, 1),
            new Anim("walk_d", new int[] { off, off + 1 }, 4),
            new Anim("walk_r", new int[] { off + 2, off + 3 }, 4),
            new Anim("walk_u", new int[] { off + 4, off + 5 }, 4),
            new Anim("walk_l", new int[] { off + 6, off + 7 }, 4)
            );


        public SuburbKiller(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(GlobalState.RNG.Next(0,7)*9), Drawing.DrawOrder.ENTITIES)
        {
            _player = p;

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
                p.ReceiveDamage(6, DamageDealer);
            }
        }
    }
}
