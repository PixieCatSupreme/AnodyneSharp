using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Apartment
{
    [NamedEntity("Gasguy"), Enemy, Collision(typeof(Player), typeof(Broom), KeepOnScreen = true, MapCollision = false)]

    public class GasGuy : HealthDropper
    {
        private int _health = 3;
        private IState _state;

        private float soundDelay;
        private float soundDelayMax;

        private Player _player;

        private bool _inDelay;
        private bool _didFirstShot;

        private EntityPool<Gas> gasClouds;

        public GasGuy(EntityPreset preset, Player player)
            : base(preset.Position, "gas_guy", 16, 24, DrawOrder.ENTITIES, 0.6f, true)
        {
            _player = player;

            AddAnimation("float", CreateAnimFrameArray(0, 1), 2, true);
            AddAnimation("release_gas", CreateAnimFrameArray(2), 20, true);
            Play("float");

            drag = new Vector2(30);

            soundDelay = 0;
            soundDelayMax = (float)(1.5 + GlobalState.RNG.NextDouble());

            gasClouds = new EntityPool<Gas>(3, () => new Gas());

            _state = new StateMachineBuilder()
                .State<TimerState>("InitialLatency")
                    .Enter((state) =>
                    {
                        state.Reset();
                        state.AddTimer(1.5f, "StartTimer");
                        _inDelay = true;
                    })
                    .Event("StartTimer", (state) =>
                    {
                        _state.ChangeState("Normal");
                    })
                    .Exit((state) =>
                    {
                        _inDelay = false;
                    })
                .End()
                .State<TimerState>("Normal")
                    .Enter((state) =>
                    {
                        state.Reset();
                        state.AddTimer(_didFirstShot ? 5f : 1f, "PreShootTimer");
                        Play("float");
                    })
                    .Event("PreShootTimer", (state) =>
                    {
                        Play("release_gas");
                        _state.ChangeState("Shoot");
                        _didFirstShot = true;
                    })
                .End()
                .State<TimerState>("Shoot")
                .Enter((state) =>
                    {
                        state.Reset();
                        state.AddTimer(1.2f, "ShootTimer");
                    })
                    .Event("ShootTimer", (state) =>
                    {
                        Play("shoot");
                        _state.ChangeState("Normal");
                        SoundManager.PlaySoundEffect("gasguy_shoot");
                        gasClouds.Spawn((s) => s.Spawn(this, _player), 3, true);
                    })
                .End()
            .Build();

            _state.ChangeState("InitialLatency");
        }

        public override void Update()
        {
            base.Update();

            _state.Update(GameTimes.DeltaTime);

            soundDelay += GameTimes.DeltaTime;

            if (soundDelay > soundDelayMax)
            {
                soundDelay = 0;
                SoundManager.PlaySoundEffect("gasguy_move");
            }

            if (!_inDelay)
            {
                MathUtilities.MoveTo(ref Position.X, _player.Position.X, 12f);
                MathUtilities.MoveTo(ref Position.Y, _player.Position.Y, 12f);
            }
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                p.ReceiveDamage(1);
            }
            else if (other is Broom)
            {
                if (!_flickering)
                {
                    _health--;
                    Flicker(0.5f);

                    MoveTowards(_player.Position, 100);
                    velocity *= -1;

                    SoundManager.PlaySoundEffect("generic_hurt");

                    if (_health == 0)
                    {
                        Die();

                        GlobalState.SpawnEntity(new Explosion(this));
                    }
                }
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return gasClouds.Entities;
        }

        [Collision(typeof(Player), typeof(Broom), KeepOnScreen = false, MapCollision = false)]
        private class Gas : Entity
        {
            public Gas()
                : base(Vector2.Zero, "gas_guy_cloud", 24, 24, DrawOrder.FG_SPRITES)
            {
                AddAnimation("gas_move", CreateAnimFrameArray(0, 1), 3);

                offset = new Vector2(4);

                Play("gas_move");

                width = height = 16;
            }

            public void Spawn(Entity parent, Entity target)
            {
                Position = parent.Position;

                MoveTowards(target.Position, 30);

                velocity.X += GlobalState.RNG.Next(-10, 11);
                velocity.Y += GlobalState.RNG.Next(-10, 11);

                Flicker(9001);

                opacity = 0.8f;
            }

            public override void Update()
            {
                base.Update();

                opacity -= 0.06f * GameTimes.DeltaTime;

                if (opacity <= 0f)
                {
                    exists = false;
                }
            }

            public override void Collided(Entity other)
            {
                if (opacity > 0.3f && other is Player p)
                {
                    p.reversed = true;
                    GlobalState.wave.active = true;
                }
                else if (other is Broom)
                {
                    opacity -= 0.025f;
                }
            }
        }
    }
}
