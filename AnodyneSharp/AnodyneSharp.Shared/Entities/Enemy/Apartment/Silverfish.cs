using AnodyneSharp.Drawing;
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
        private Player _player;
        private IEnumerator _stateLogic;

        public Silverfish(EntityPreset preset, Player player)
            : base(preset, preset.Position, "silverfish", 16, 16, DrawOrder.ENTITIES)
        {
            _player = player;

            int o = GlobalState.IsCell ? 6 : 0;

            AddAnimation("move_l", CreateAnimFrameArray(6 + o, 7 + o), 7, true);
            AddAnimation("move_d", CreateAnimFrameArray(4 + o, 5 + o), 7, true);
            AddAnimation("move_r", CreateAnimFrameArray(6 + o, 7 + o), 7, true); // DEFAULT: RIGHT
            AddAnimation("move_u", CreateAnimFrameArray(8 + o, 9 + o), 7, true);
            AddAnimation("idle_d", CreateAnimFrameArray(4 + o), 12, true);
            AddAnimation("idle_u", CreateAnimFrameArray(8 + o), 12, true);
            AddAnimation("idle_r", CreateAnimFrameArray(6 + o), 12, true); // DEFAULT: RIGHT
            AddAnimation("idle_l", CreateAnimFrameArray(6 + o), 12, true);

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
                p.ReceiveDamage(1);
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

                FlipFacing();

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
