using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy
{
    [NamedEntity, Enemy, Collision(typeof(Player),typeof(Broom),MapCollision = true)]
    public class Slime : Entity
    {
        private enum SlimeType
        {
            Normal,
            //Key, unused
            //Rise, unused
            Bullet = 3
        }

        EntityPreset _preset;

        private SlimeType _type;

        private int _health = 2;

        private const float _moveTimerMax = 0.5f;
        private const float _shootTimerMax = 1.8f;
        private float _moveTimer = _moveTimerMax;
        private float _shootTimer = _shootTimerMax;

        private bool move_frame_sound_sync = false;

        private float _speed = 20f;


        private EntityPool<Goo> goos;
        private EntityPool<Bullet> bullets;

        private Player target;

        public Slime(EntityPreset preset, Player p) 
            : base(preset.Position, "slime", 16,16, Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;

            _type = _preset.Frame == 3 ? SlimeType.Bullet : SlimeType.Normal;

            AddAnimation("Move", CreateAnimFrameArray(0, 1), 3);
            AddAnimation("Hurt", CreateAnimFrameArray(0, 8, 0, 8), 15);
            AddAnimation("Dying", CreateAnimFrameArray(0, 8, 0, 8), 12, false);

            Play("Move");

            goos = new EntityPool<Goo>(8, () => new Goo());
            target = p;

            if (_type == SlimeType.Bullet)
            {
                bullets = new EntityPool<Bullet>(4, () => new Bullet());
                _speed *= 2;
            }
        }

        public override void Update()
        {
            //Using animation state as slime's state because it's easier than tracking multiple timers
            switch(_curAnim.name)
            {
                case "Move":
                    if (_curFrame == 1 && !move_frame_sound_sync)
                    {
                        move_frame_sound_sync = true;
                        SoundManager.PlaySoundEffect("slime_walk");
                    }
                    else if (_curFrame == 0)
                    {
                        move_frame_sound_sync = false;
                    }

                    _moveTimer -= GameTimes.DeltaTime;
                    if(_moveTimer <= 0)
                    {
                        _moveTimer = _moveTimerMax;
                        if(_curFrame == 1)
                        {
                            //Make it more likely for slimes to stand still periodically
                            velocity = Vector2.Zero;
                        }
                        else
                        {
                            velocity = new Vector2((float)GlobalState.RNG.NextDouble(),(float)GlobalState.RNG.NextDouble()) - Vector2.One/2f;
                            velocity *= _speed;
                        }
                    }

                    if (_type == SlimeType.Bullet)
                    {
                        Shoot();
                    }
                    break;
                case "Hurt":
                    if (finished) Play("Move");
                    break;
                case "Dying":
                    if(finished)
                    {
                        exists = _preset.Alive = false;
                    }
                    break;
            }
            base.Update();
        }

        public override void Collided(Entity other)
        {
            if(other is Player p && _curAnim.name != "Dying")
            {
                p.ReceiveDamage(1);
            }
            else if(other is Broom b && _curAnim.name == "Move")
            {
                SoundManager.PlaySoundEffect("hit_slime");

                goos.Spawn(g => g.Spawn(this), 2);

                _health -= 1;
                if(_health == 0)
                {
                    Play("Dying");
                    velocity = Vector2.Zero;
                }
                else
                {
                    Play("Hurt");

                    velocity = FacingDirection(b.facing) * 100;
                }
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            if (_type == SlimeType.Normal)
            {
                return goos.Entities;
            }
            else
            {
                return goos.Entities.Concat(bullets.Entities);
            }

        }

        private void Shoot()
        {
            _shootTimer -= GameTimes.DeltaTime;

            if (_shootTimer < 0)
            {
                _shootTimer = _shootTimerMax;
                bullets.Spawn(b => b.Spawn(this, target));
                SoundManager.PlaySoundEffect("slime_shoot");
            }
        }

        [Collision(MapCollision = true)]
        private class Goo : Entity
        {
            private Parabola_Thing parabola;

            public Goo() : base(Vector2.Zero, "slime_goo", 6, 6, DrawOrder.PARTICLES)
            {
                AddAnimation("move", CreateAnimFrameArray(0, 1, 2, 3, 1, 3, 1, 2, 1, 0), GlobalState.RNG.Next(5, 10));
                shadow = new Shadow(this, Vector2.Zero, ShadowType.Tiny);

                parabola = new Parabola_Thing(this, 16, 0.8f + 0.3f * (float)GlobalState.RNG.NextDouble());
            }

            public void Spawn(Slime parent)
            {
                Position = parent.Position;
                velocity.X = MathUtilities.OneRandomOf(-1, 1) * (10 + 5 * (float)GlobalState.RNG.NextDouble());
                velocity.Y = MathUtilities.OneRandomOf(-1, 1) * (10 + 5 * (float)GlobalState.RNG.NextDouble());
                Play("move");
                shadow.exists = true;
                parabola.ResetTime();
                _opacity = 1.0f;
            }

            public override void Update()
            {
                base.Update();
                if (parabola.Tick())
                {
                    if (shadow.exists)
                    {
                        SoundManager.PlaySoundEffect("slime_splash");
                        shadow.exists = false;
                        _curAnim = null;
                        velocity = Vector2.Zero;
                    }
                    else
                    {
                        _opacity -= 0.05f;
                        if (_opacity <= 0) exists = false;
                    }
                }
            }
        }
        [Collision(typeof(Player), MapCollision = true)]
        private class Bullet : Entity
        {
            public Bullet()
                : base(Vector2.Zero, "slime_bullet", 8, 8, DrawOrder.PARTICLES)
            {
                AddAnimation("move", CreateAnimFrameArray(0, 1), GlobalState.RNG.Next(5, 10));
            }

            public void Spawn(Slime parent, Player target)
            {
                Position = parent.Position;
                velocity = Vector2.Normalize(target.Position - parent.Position) * 40;

                Play("move");
                _opacity = 1.0f;
            }

            public override void Update()
            {
                base.Update();

                _opacity -= 0.39f * GameTimes.DeltaTime;

                if (touching != Touching.NONE || _opacity < 0.3f)
                {
                    exists = false;
                }
            }

            public override void Collided(Entity other)
            {
                if (other is Player p && p.state != PlayerState.AIR)
                {
                    p.ReceiveDamage(1);
                    exists = false;
                }
            }
        }
    }
}