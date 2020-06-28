using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy
{
    [NamedEntity, Enemy, Collision(typeof(Player),typeof(Broom),MapCollision = true)]
    public class Slime : Entity
    {
        EntityPreset _preset;

        private int _health = 2;

        private float _deathTimer = 0.5f;
        private const float _moveTimerMax = 0.5f;
        private float _moveTimer = _moveTimerMax;

        private bool move_frame_sound_sync = false;

        private float _speed = 20f;

        private EntityPool<Goo> goos;

        private Player target;


        public Slime(EntityPreset preset, Player p) 
            : base(preset.Position, "slime", 16,16, Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;

            AddAnimation("Move", CreateAnimFrameArray(0, 1), 3);
            AddAnimation("Hurt", CreateAnimFrameArray(0, 8, 0, 8), 15);
            AddAnimation("Dying", CreateAnimFrameArray(0, 8, 0, 8, 15, 9, 9), 12, false);

            Play("Move");

            goos = new EntityPool<Goo>(8, () => new Goo());
            target = p;
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
                    break;
                case "Hurt":
                    if (finished) Play("Move");
                    break;
                case "Dying":
                    if(finished && _preset.Alive)
                    {
                        _deathTimer -= GameTimes.DeltaTime;
                        if (_deathTimer <= 0)
                        {
                            exists = _preset.Alive = false;
                        }
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
            return goos.Entities;
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
    }
}