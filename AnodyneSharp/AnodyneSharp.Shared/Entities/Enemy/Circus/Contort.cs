using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Circus
{
    [NamedEntity(), Enemy, Collision(typeof(Player), typeof(Broom), KeepOnScreen = true, MapCollision = true)]
    class Contort : Entity
    {
        private const float _startVel = 35;

        private float DeathTimer = 1;

        private List<ContortSmall> _smalls;

        private EntityPreset _preset;

        private int health = 3;

        public Contort(EntityPreset preset, Player p)
            : base(preset.Position, new AnimatedSpriteRenderer("contort_big", 16, 32, new Anim("move",new int[] {0,1,2,1},9)), DrawOrder.ENTITIES)
        {
            height = 12;
            width = 12;
            offset = new Vector2(2, 20);

            _preset = preset;

            velocity = new Vector2(
                _startVel * (GlobalState.RNG.NextDouble() > 0.5 ? 1 : -1),
                _startVel * (GlobalState.RNG.NextDouble() > 0.5 ? 1 : -1)
                );

            _smalls = new List<ContortSmall>()
            {
                new ContortSmall(preset.Position, 0, 0),
                new ContortSmall(preset.Position, 2, 2),
                new ContortSmall(preset.Position, 1, 4),
            };
        }

        public override void Update()
        {
            base.Update();

            if (DeathTimer > 0)
            {
                if (touching != Touching.NONE)
                {
                    if (touching.HasFlag(Touching.RIGHT) || touching.HasFlag(Touching.LEFT))
                    {
                        velocity.X *= -1;
                    }

                    if (touching.HasFlag(Touching.UP) || touching.HasFlag(Touching.DOWN))
                    {
                        velocity.Y *= -1;
                    }
                }

                if (health == 0)
                {
                    DeathTimer -= GameTimes.DeltaTime;

                    if (DeathTimer <= 0)
                    {
                        Flicker(0);

                        Split();
                    }
                }
            }
            else if (!_smalls.Any(s => s.exists))
            {
                _preset.Alive = exists = false;
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (health == 0)
            {
                return;
            }

            if (other is Broom && !_flickering)
            {
                health -= 1;

                if (health == 0)
                {
                    velocity = Vector2.Zero;
                }

                Flicker(1.5f);
            }
            else if (other is Player p)
            {
                p.ReceiveDamage(1);
            }
        }

        private void Split()
        {
            visible = false;

            GlobalState.SpawnEntity(new Explosion(this));

            foreach (var small in _smalls)
            {
                small.Position = Position;
                small.exists = true;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return _smalls;
        }

        public override void Fall(Vector2 fallPoint)
        { }

        [Collision(typeof(Player), typeof(Broom), KeepOnScreen = true, MapCollision = true)]
        class ContortSmall : HealthDropper
        {
            private const float _startVel = 35;
            private const float SmallRollTimerMax = 0.8f;

            private float SmallRollTimer = 0;


            private Parabola_Thing _parabola;

            public ContortSmall(Vector2 pos, int i, int startFrame)
            : base(null, pos, new("contort_small", 16, 16, new Anim("move",new int[] {startFrame,startFrame+1},9)), DrawOrder.ENTITIES, 0.4f)
            {
                exists = false;

                _parabola = new Parabola_Thing(this, 16 + 8 * i, 1.2f);
                shadow = new Shadow(this, new Vector2(0, -5), ShadowType.Normal);

                velocity = new Vector2(RandomVelocity(), RandomVelocity());

                Flicker(1f);
            }

            public override void Update()
            {
                base.Update();

                if (_parabola.Tick())
                {
                    shadow.visible = false;

                    SmallRollTimer += GameTimes.DeltaTime;

                    if (SmallRollTimer >= SmallRollTimerMax)
                    {
                        SmallRollTimer = 0;

                        velocity = new Vector2(RandomVelocity(), RandomVelocity());
                    }
                }

                if (touching != Touching.NONE)
                {
                    if (touching.HasFlag(Touching.RIGHT) || touching.HasFlag(Touching.LEFT))
                    {
                        velocity.X *= -1;
                    }

                    if (touching.HasFlag(Touching.UP) || touching.HasFlag(Touching.DOWN))
                    {
                        velocity.Y *= -1;
                    }
                }
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);

                if (other is Broom b && !_flickering)
                {
                    Die();

                    GlobalState.SpawnEntity(new Explosion(this));
                }
                else if (other is Player p)
                {
                    p.ReceiveDamage(1);
                }
            }

            private static float RandomVelocity()
            {
                return _startVel * (GlobalState.RNG.NextDouble() > 0.5 ? 1 : -1) + GlobalState.RNG.Next(-5, 5);
            }
        }
    }
}
