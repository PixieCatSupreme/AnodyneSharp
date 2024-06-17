using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Apartment
{
    [NamedEntity("Silverfish"), Enemy, Collision(typeof(Player), typeof(Broom), KeepOnScreen = true, MapCollision = true)]
    public class Silverfish : HealthDropper
    {
        public const string DamageDealer = "Silverfish";
        private Player _player;
        private IEnumerator _stateLogic;

        public static AnimatedSpriteRenderer GetSprite(int o) => new("silverfish", 16, 16,
            new Anim("move_l", new int[] { 6 + o, 7 + o }, 7),
            new Anim("move_d", new int[] { 4 + o, 5 + o }, 7),
            new Anim("move_r", new int[] { 6 + o, 7 + o }, 7), // DEFAULT: RIGHT
            new Anim("move_u", new int[] { 8 + o, 9 + o }, 7),
            new Anim("idle_r", new int[] { 6 + o }, 12), // DEFAULT: RIGHT
            new Anim("idle_d", new int[] { 4 + o }, 12),
            new Anim("idle_u", new int[] { 8 + o }, 12),
            new Anim("idle_l", new int[] { 6 + o }, 12));

        public Silverfish(EntityPreset preset, Player player)
            : base(preset, preset.Position, GetSprite(GlobalState.IsCell ? 6 : 0), DrawOrder.ENTITIES)
        {
            _player = player;

            facing = preset.Frame switch
            {
                0 => Facing.LEFT,
                1 => Facing.DOWN,
                2 => Facing.RIGHT,
                _ => Facing.UP,
            };

            PlayFacing("idle");

            _stateLogic = StateLogic();
        }

        public override void Update()
        {
            base.Update();

            _stateLogic.MoveNext();
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Player p)
            {
                p.ReceiveDamage(1, DamageDealer);
            }
            else if (other is Broom)
            {
                GetHit();
            }
        }

        public void GetHit()
        {
            Die();
        }

        protected override void Die()
        {
            base.Die();

            GlobalState.SpawnEntity(new Explosion(this));
        }

        protected override void AnimationChanged(string name)
        {
            if (name.EndsWith("_l"))
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            }
        }

        private IEnumerator StateLogic()
        {
            //normal
            while (!SeePlayer())
            {
                yield return null;
            }

            while (true)
            {
                SoundManager.PlaySoundEffect("sf_move");

                facing = FlipFacing(facing);

                PlayFacing("move");

                velocity = FacingDirection(facing) * 50;

                while (touching == Touching.NONE)
                {
                    yield return null;
                }

                touching = Touching.NONE;

                velocity = Vector2.Zero;

                PlayFacing("idle");

                float turnTimer = 0;

                while (!SeePlayer())
                {
                    turnTimer += GameTimes.DeltaTime;

                    if (turnTimer > 0.8f)
                    {
                        turnTimer = 0;

                        facing = facing switch
                        {
                            Facing.LEFT => Facing.UP,
                            Facing.RIGHT => Facing.DOWN,
                            Facing.UP => Facing.RIGHT,
                            _ => Facing.LEFT,
                        };

                        PlayFacing("idle");
                    }

                    yield return null;
                }

                FaceTowards(_player.Position); //Only need to do this before flipping to run away when in turning mode, not on first run
            }
        }


        private bool SeePlayer()
        {
            bool seeX = _player.Hitbox.Top <= Hitbox.Bottom && _player.Hitbox.Bottom >= Hitbox.Top;
            bool seeY = _player.Hitbox.Left <= Hitbox.Right && _player.Hitbox.Right >= Hitbox.Left;

            if ((_player.Position - Position).Length() < 30)
            {
                return true;
            }

            return facing switch
            {
                Facing.LEFT => seeX && _player.Position.X < Position.X,
                Facing.RIGHT => seeX && _player.Position.X > Position.X,
                Facing.UP => seeY && _player.Position.Y < Position.Y,
                _ => seeY && _player.Position.Y > Position.Y,
            };
        }
    }
}
