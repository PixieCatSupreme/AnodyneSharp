using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Bedroom
{
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom), MapCollision = true, KeepOnScreen = true)]
    public class Shieldy : HealthDropper
    {
        private enum State
        {
            NOTHING,
            LEFT = 4,
            UP,
            RIGHT,
            DOWN
        }

        private const int _velocityMax = 30;
        private const float hitTimerMax = 0.4f;

        private int _health = 2;
        private float _hitTimer = 0;

        public Shieldy(EntityPreset preset, Player player)
            : base(preset.Position + new Vector2(2, 0), "shieldy", 16, 16, DrawOrder.ENTITIES)
        {
            Solid = true;
            height = 10;
            width = 10;

            offset = new Vector2(3, 4);

            AddAnimation("walk", CreateAnimFrameArray(1, 2, 1, 0), 5, true);
            //addAnimation("unhurt", [4], 7, true);
            AddAnimation("front_hit", CreateAnimFrameArray(16, 17, 18, 1), 12, false);
            AddAnimation("back_hit", CreateAnimFrameArray(13, 1), 12);

            Play("walk");

            State state = (State)preset.Frame;

            switch (state)
            {
                case State.LEFT:
                    velocity = new Vector2(-_velocityMax, 0);
                    break;
                case State.RIGHT:
                    velocity = new Vector2(_velocityMax, 0);
                    break;
                case State.UP:
                    velocity = new Vector2(0, -_velocityMax);
                    break;
                case State.DOWN:
                    velocity = new Vector2(0, _velocityMax);
                    break;
                default:
                    break;
            }
        }

        internal void ReceiveDamage(int damage)
        {
            _health -= damage;
        }

        public override void Update()
        {
            base.Update();

            if (_hitTimer > 0)
            {
                _hitTimer -= GameTimes.DeltaTime;
                if (_hitTimer <= 0)
                {
                    Play("walk");
                }
            }

            if (_health <= 0)
            {
                GlobalState.SpawnEntity(new Explosion(this));
                Die();
            }

            if (touching != Touching.NONE)
            {
                velocity *= -1;
            }
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                p.ReceiveDamage(1);
            }
            else if (other is Broom b)
            {
                if (_hitTimer <= 0)
                {
                    if (b.facing == Facing.DOWN)
                    {
                        _health--;
                        Flicker(hitTimerMax / 2);
                        Play("back_hit");
                        SoundManager.PlaySoundEffect("shieldy-hit");
                    }
                    else
                    {
                        Play("front_hit");
                        SoundManager.PlaySoundEffect("shieldy_ineffective");

                        Vector2 newPos = Position;

                        switch (b.facing)
                        {
                            case Facing.LEFT:
                                newPos.X -= 8;
                                break;
                            case Facing.UP:
                                newPos.Y -= 8;
                                break;
                            case Facing.RIGHT:
                                newPos.X += 8;
                                break;
                        }

                        Touching checkTL = GlobalState.CheckTile(newPos);
                        Touching checkBR = GlobalState.CheckTile(newPos + new Vector2(width,height));

                        if (checkTL == Touching.NONE && checkBR == Touching.NONE)
                        {
                            Position = newPos;
                        }

                    }

                    _hitTimer = hitTimerMax;
                }

            }
        }
    }
}
