using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Crowd
{
    //TODO: collide with switch pillar
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom), typeof(Gate), KeepOnScreen = true, MapCollision = true )]
    public class Dog : HealthDropper
    {
        private int _health = 3;
        private IState _state;

        private int pace_vel = 20;

        private DogRegion _activeRegion;
        private Player _target;
        private bool _isSuperDog;

        private float _hitTimer;

        public Dog(EntityPreset preset, Player p)
            : base(preset.Position, "dog", 16, 16, Drawing.DrawOrder.ENTITIES, 1)
        {
            width = height = 12;
            offset = new Vector2(2);
            drag.X = 20;

            AddAnimation("stop", CreateAnimFrameArray(0));
            AddAnimation("walk", CreateAnimFrameArray(2, 3), 4);
            AddAnimation("alert", CreateAnimFrameArray(4, 5), 4);
            AddAnimation("attack", CreateAnimFrameArray(6, 7), 6);

            _activeRegion = new DogRegion(this);
            _target = p;

            _isSuperDog = preset.TypeValue == "1";

            _hitTimer = 0;

            _state = new StateMachineBuilder()
                .State<TimerState>("Pace")
                    .Enter((state) =>
                    {
                        state.Reset();
                        state.AddTimer(1f, "MoveTimer");
                        _activeRegion.exists = true;

                        velocity = Vector2.Zero;
                    })
                    .Event("MoveTimer", (state) =>
                     {
                         if (GlobalState.RNG.NextDouble() > 0.33)
                         {
                             Play("walk");
                             velocity.X = pace_vel;
                             SetFlip();
                         }
                         else if (GlobalState.RNG.NextDouble() > 0.5)
                         {
                             Play("walk");
                             velocity.X = -pace_vel;
                             SetFlip();
                         }
                         else
                         {
                             Play("stop");
                             velocity.X = 0;
                         }
                     })
                    .Event("PlayerInArea", (state) =>
                      {
                          state.Parent.ChangeState("Alert");
                      })
                    .Exit((state) =>
                    {
                        _activeRegion.exists = false;
                    })
                .End()
                .State<TimerState>("Alert")
                    .Enter((state) =>
                    {
                        state.Reset();
                        state.AddTimer(1f, "AlertTimer");

                        velocity = Vector2.Zero;
                        Play("alert");
                        SoundManager.PlaySoundEffect("dog_bark");
                    })
                    .Event("AlertTimer", (state) =>
                    {
                        state.Parent.ChangeState("Attack");
                    })
                .End()
                .State<AttackState>("Attack")
                    .Enter((state) =>
                    {
                        state.attackPhase = 0;
                        Vector2 tPos = _target.Position;
                        state.targetPos = new Vector2(tPos.X - GlobalState.RNG.Next(38, 51), tPos.Y - 8);

                        if (!_isSuperDog)
                        {
                            state.attackPositions[0] = new Vector2(tPos.X + GlobalState.RNG.Next(30, 43), tPos.Y - 8);
                            state.attackPositions[1] = new Vector2(tPos.X - GlobalState.RNG.Next(38, 51), tPos.Y - 8);
                        }

                        MoveTowards(state.targetPos, state.AttackSpeed);

                        Play("attack");

                        SetFlip();
                    })
                    .Event("AttackTimer", (state) =>
                    {
                        state.attackPhase++;

                        if (state.attackPhase > (_isSuperDog ? 4 : 2))
                        {
                            state.Parent.ChangeState("Pace");
                            return;
                        }

                        SoundManager.PlaySoundEffect("dog_dash");

                        if (_isSuperDog)
                        {
                            state.targetPos = _target.Position;

                            if (_target.Position.X > Position.X)
                            {
                                state.targetPos += new Vector2(25,8);
                            }
                            else
                            {
                                state.targetPos -= new Vector2(28, 8);
                            }
                        }
                        else
                        {
                            state.targetPos = state.AttackPos;
                        }

                        MoveTowards(state.targetPos, state.AttackSpeed);

                        SetFlip();
                    })
                .End()
                .Build();

            _state.ChangeState("Pace");
        }

        public override void Update()
        {
            base.Update();
            _state.Update(GameTimes.DeltaTime);

            if (_hitTimer > 0)
            {
                _hitTimer -= GameTimes.DeltaTime;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(_activeRegion, 1);
        }

        public override void Collided(Entity other)
        {
            if (other is Player p && p.state != PlayerState.AIR)
            {
                p.ReceiveDamage(1);
            }
            else if (other is Broom)
            {
                if (_hitTimer <= 0)
                {
                    _health--;
                    _hitTimer = 1.5f;
                    Flicker(_hitTimer);

                    SoundManager.PlaySoundEffect("broom_hit");

                    if (_health == 0)
                    {
                        Die();

                        GlobalState.SpawnEntity(new Explosion(this));
                    }
                }
            }
            else
            {
                Separate(this, other);
            }
        }


        private void SetFlip()
        {
            if (velocity.X > 0)
            {
                _flip = SpriteEffects.None;
            }
            else
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
        }

        private class AttackState : TimerState
        {
            public static readonly float[] attackSpeeds = new float[]
            {
                1.1f, 1.4f, 1.4f, 1.6f, 1.7f
            };

            public float AttackSpeed => 60 * attackSpeeds[attackPhase];
            public Vector2 AttackPos => attackPositions[attackPhase-1];

            public Vector2 targetPos;
            public Vector2[] attackPositions;

            public int attackPhase;

            public AttackState()
            {
                AddTimer(1f, "AttackTimer");

                attackPositions = new Vector2[2];
            }
        }

        [Collision(typeof(Player))]
        private class DogRegion : Entity
        {
            private Dog _parent;

            public DogRegion(Dog parent)
                : base(Vector2.Zero, DrawOrder.ENTITIES)
            {
                visible = false;
                _parent = parent;

                width = height = 96;
            }

            public override void Collided(Entity other)
            {
                _parent._state.TriggerEvent("PlayerInArea");
            }

            public override void Update()
            {
                Position = _parent.Center - new Vector2(width, height) / 2;
            }
        }
    }
}
