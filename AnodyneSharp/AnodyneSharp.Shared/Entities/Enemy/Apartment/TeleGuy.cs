using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Apartment
{
    [NamedEntity("Teleguy"), Enemy, Collision(typeof(Player), typeof(Broom))]
    class TeleGuy : HealthDropper
    {
        EntityPreset _preset;
        Player _player;

        private IState _state;

        private Vector2 atkPoint;

        public TeleGuy(EntityPreset preset, Player p)
            : base(preset.Position, "teleport_guy", 16, 24, Drawing.DrawOrder.ENTITIES, 0.7f)
        {
            _preset = preset;
            _player = p;

            offset.Y = 6;
            height = 16;

            AddAnimation("idle_d", CreateAnimFrameArray(0, 1), 3);
            AddAnimation("idle_r", CreateAnimFrameArray(2, 3), 3); //DEFAULT: RIGHT
            AddAnimation("idle_u", CreateAnimFrameArray(4, 5), 3);
            AddAnimation("idle_l", CreateAnimFrameArray(2, 3), 3);
            AddAnimation("poof", CreateAnimFrameArray(6, 7, 8, 9), 12, false);
            AddAnimation("unpoof", CreateAnimFrameArray(8, 7, 6), 12, false);
            AddAnimation("dying", CreateAnimFrameArray(0));

            Play("idle_d");

            immovable = true;

            _state = new StateMachineBuilder()
                .State("Idle")
                    .Event<CollisionEvent<Player>>("Player", (state, p) => p.entity.ReceiveDamage(1))
                    .Event<CollisionEvent<Broom>>("Broom", (state, b) =>
                    {
                        _state.ChangeState("Teleporting");
                    })
                    .Update((state, time) =>
                    {
                        FaceTowards(_player.Position);

                        PlayFacing("idle");
                    })
                    .End()
                .State<TimerState>("Teleporting")
                    .Enter((state) =>
                    {
                        state.Reset();

                        SoundManager.PlaySoundEffect("teleguy_up");

                        Play("poof");

                        state.AddTimer(0.5f, "TeleportTimer");
                    })
                    .Event("TeleportTimer", (state) =>
                    {
                        SoundManager.PlaySoundEffect("teleguy_down");

                        atkPoint = Position;

                        Teleport();

                        Play("unpoof");

                        _state.ChangeState("PreAttack");
                    })
                    .End()
                    .State<TimerState>("PreAttack")
                    .Enter((state) =>
                    {
                        state.Reset();

                        FaceTowards(_player.Position);

                        state.AddTimer(0.5f, "PreAttackTimer");
                    })
                    .Event("PreAttackTimer", (state) =>
                    {
                        PlayFacing("idle");

                        _state.ChangeState("Attacking");
                    })
                    .End()
                .State<TimerState>("Attacking")
                    .Enter((state) =>
                    {
                        state.Reset();

                        state.AddTimer(1.5f, "AttackTimer");

                        MoveTowards(atkPoint, 60.0f);
                    })
                    .Update((state, time) =>
                    {
                        if ((atkPoint - Position).Length() <= 1)
                        {
                            Position = atkPoint;
                            velocity = Vector2.Zero;
                        }
                    })
                    .Event<CollisionEvent<Player>>("Player", (state, p) => p.entity.ReceiveDamage(1))
                    .Event<CollisionEvent<Broom>>("Broom", (state, b) =>
                    {
                        if (GlobalState.RNG.NextDouble() < 0.5f)
                        {
                            _state.ChangeState("Teleporting");
                        }
                        else
                        {
                            _preset.Alive = false;

                            Die();

                            GlobalState.SpawnEntity(new Explosion(this));
                        }

                    })
                    .Event("AttackTimer", (state) =>
                    {
                        _state.ChangeState("Idle");
                    })
                    .Exit((state) =>
                    {
                        velocity = Vector2.Zero;
                    })
                    .End()
                .Build();

            _state.ChangeState("Idle");
        }

        public override void Update()
        {
            base.Update();

            _state.Update(GameTimes.DeltaTime);
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                _state.TriggerEvent("Player", new CollisionEvent<Player>() { entity = p });
            }
            else if (other is Broom b)
            {
                _state.TriggerEvent("Broom", new CollisionEvent<Broom>() { entity = b });
            }
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


        private void Teleport()
        {

            if (_player.facing == Facing.LEFT)
            {
                Position = _player.Position + new Vector2(16, 0);

                facing = Facing.LEFT;
            }
            else if (_player.facing == Facing.RIGHT)
            {
                Position = _player.Position + new Vector2(-16, 0);

                facing = Facing.RIGHT;
            }
            else if (_player.facing == Facing.UP)
            {
                Position = _player.Position + new Vector2(0, 17);
                facing = Facing.UP;
            }
            else if (_player.facing == Facing.DOWN)
            {
                Position = _player.Position + new Vector2(0, -16);
                facing = Facing.DOWN;
            }
        }
    }
}
