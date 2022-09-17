using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Hotel
{
    [NamedEntity, Enemy, Collision(typeof(Broom))]
    public class Burst_Plant : HealthDropper
    {
        int _health = 2;
        IState _state;

        EntityPool<BurstBullet> _bullets;

        public Burst_Plant(EntityPreset preset, Player p) : base(preset, preset.Position, "burst_plant", 16, 16, Drawing.DrawOrder.ENTITIES, 0.7f)
        {
            AddAnimation("idle", CreateAnimFrameArray(0));
            AddAnimation("charging", CreateAnimFrameArray(0, 1), 8);
            AddAnimation("shoot", CreateAnimFrameArray(3));
            Play("idle");
            immovable = true;

            _bullets = new(8, () => new());

            _state = new StateMachineBuilder()
                .State<TimerState>("Initial")
                    .Enter((s) => s.AddTimer(0.4f, "goToIdle"))
                    .Event("goToIdle", (s) => _state.ChangeState("Idle"))
                .End()
                .State<TimerState>("Idle")
                    .Enter((s) =>
                    {
                        Play("idle");
                        s.Reset();
                        s.AddTimer(0.4f + (float)GlobalState.RNG.NextDouble(), "idleTimer");
                    })
                    .Event("idleTimer", (s) => _state.ChangeState("Charging"))
                .End()
                .State<ChargeState>("Charging")
                    .Enter((s) => Play("charging"))
                    .Event("Fire", (s) =>
                    {
                        int sign = MathUtilities.OneRandomOf(-1, 1);
                        float max_vel = 10 + 30 * (float)GlobalState.RNG.NextDouble();
                        _bullets.Spawn((b) => b.SpawnToPlayer(this, max_vel, p)); //one straight to the player
                        _bullets.Spawn((b) => b.Spawn(this, max_vel, sign), 7);
                        Play("shoot");
                        SoundManager.PlaySoundEffect("bubble_triple");
                        _state.ChangeState("BulletsFlying");
                    })
                .End()
                .State("BulletsFlying")
                    .Condition(() => _bullets.Alive == 0, (s) => _state.ChangeState("Idle"))
                .End()
            .Build();
            _state.ChangeState("Initial");
        }

        public override void Update()
        {
            base.Update();
            _state.Update(GameTimes.DeltaTime);
        }

        public override void Collided(Entity other)
        {
            if (!_flickering)
            {
                --_health;
                Flicker(1);

                SoundManager.PlaySoundEffect("broom_hit");

                if (_health <= 0)
                {
                    GlobalState.SpawnEntity(new Explosion(this));
                    Die();
                    foreach(Entity b in _bullets.Entities)
                    {
                        b.exists = false;
                    }
                }
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(_bullets.Entities);
        }

        class ChargeState : TimerState
        {
            public ChargeState()
            {
                AddTimer(0.5f, "Fire");
            }
        }

        [Collision(typeof(Player))]
        private class BurstBullet : Entity
        {
            Parabola_Thing parabola;

            IState _state;

            public BurstBullet() : base(Vector2.Zero, "burst_plant_bullet", 8, 8, Drawing.DrawOrder.FG_SPRITES)
            {
                shadow = new Shadow(this, new Vector2(3,2), ShadowType.Normal);

                AddAnimation("move",    CreateAnimFrameArray(0, 1), 12);
                AddAnimation("explode", CreateAnimFrameArray(2, 3, 4, 4), 10, false);

                _state = new StateMachineBuilder()
                    .State("Move")
                        .Enter((state) =>
                        {
                            Play("move");
                            shadow.exists = true;
                            opacity = 1.0f;
                        })
                        .Update((state, time) =>
                        {
                            if (parabola.Tick())
                            {
                                state.Parent.ChangeState("Splash");
                            }
                        })
                    .End()
                    .State("Splash")
                        .Enter((state) =>
                        {
                            shadow.exists = false;
                            Play("explode");
                            velocity = Vector2.Zero;
                        })
                        .Condition(() => _curAnim.Finished, (state) =>
                        {
                            exists = false;
                            SoundManager.PlaySoundEffect("4sht_pop");
                        })
                    .End()
                    .Build();
            }

            public override void Update()
            {
                base.Update();
                _state.Update(GameTimes.DeltaTime);
            }

            public void SpawnToPlayer(Burst_Plant parent, float vel, Player p)
            {
                Position = parent.Position + Vector2.One * 5;
                parabola = new Parabola_Thing(this, GlobalState.RNG.Next(24, 34), 1.5f);
                MoveTowards(p.Center, vel * 2.5f);
                _state.ChangeState("Move");
            }

            public void Spawn(Burst_Plant parent, float max_vel, int sign)
            {
                Position = parent.Position + Vector2.One * 5;
                parabola = new Parabola_Thing(this, GlobalState.RNG.Next(24, 34), 0.7f + (float)GlobalState.RNG.NextDouble());
                velocity.X = -max_vel + 2 * max_vel * (float)GlobalState.RNG.NextDouble();
                velocity.Y = sign * MathF.Sqrt(max_vel * max_vel - velocity.X * velocity.X);
                _state.ChangeState("Move");
            }

            public override void Collided(Entity other)
            {
                if(other is Player p && offset.Y <= 4 && velocity != Vector2.Zero)
                {
                    p.ReceiveDamage(1);
                }
            }
        }
    }
}
