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
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Circus
{
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom), MapCollision = true, KeepOnScreen = true)]
    public class Lion : Entity
    {
        private class PaceState : TimerState
        {
            public bool move_frame_sound_sync = false;
            public DateTime exit_time = DateTime.Now;
            public PaceState()
            {
                AddTimer(0.8f, "PaceTimer");
            }
        }

        private EntityPool<Fireball> fireballs;
        private Parabola_Thing _parabola;

        private int _health = 4;
        private IState _state;

        private Shadow _shadowV;
        private Shadow _shadowH;


        public Lion(EntityPreset preset, Player player)
            : base(preset.Position, "lion", 32, 32, DrawOrder.ENTITIES)
        {
            fireballs = new EntityPool<Fireball>(10, () => new Fireball());
            _parabola = new Parabola_Thing(this, 12, 1);

            _shadowV = new Shadow(this, new Vector2(-2, -21), ShadowType.BigVertical);
            _shadowH = new Shadow(this, new Vector2(0, -12), ShadowType.Big);

            AddAnimation("walk_l", CreateAnimFrameArray(0, 1), 5);
            AddAnimation("walk_r", CreateAnimFrameArray(0, 1), 5);
            AddAnimation("walk_d", CreateAnimFrameArray(10, 11), 5);
            AddAnimation("walk_u", CreateAnimFrameArray(5, 6), 5);

            AddAnimation("warn_l", CreateAnimFrameArray(3), 5, true);
            AddAnimation("warn_r", CreateAnimFrameArray(3), 5, true);

            AddAnimation("shoot_l", CreateAnimFrameArray(2), 15, true);
            AddAnimation("shoot_r", CreateAnimFrameArray(2), 15, true);
            AddAnimation("shoot_d", CreateAnimFrameArray(12), 15, true);
            AddAnimation("shoot_u", CreateAnimFrameArray(7), 15, true);

            AddAnimation("pounce_r", CreateAnimFrameArray(4));
            AddAnimation("pounce_l", CreateAnimFrameArray(4));
            AddAnimation("pounce_u", CreateAnimFrameArray(9));
            AddAnimation("pounce_d", CreateAnimFrameArray(14));

            Play("walk_r");

            _state = new StateMachineBuilder()
                .State<PaceState>("Pace")
                    .Event("PaceTimer", (state) =>
                    {
                        double r = GlobalState.RNG.NextDouble();

                        if (r < 0.25)
                        {
                            //_state.ChangeState("Shoot");
                        }
                        else if (r < 0.55)
                        {
                            //_state.ChangeState("Charge");
                        }
                        else
                        {
                            double paceR = GlobalState.RNG.Next(0, 6);

                            if (paceR < 4)
                            {
                                facing = paceR switch
                                {
                                    0 =>  Facing.RIGHT,
                                    1 => Facing.DOWN,
                                    2 => Facing.LEFT,
                                    _ => Facing.UP,
                                };

                                PlayFacing("walk");

                                velocity = FacingDirection(facing) * 43;
                            }
                        }
                    })
                    .Event<CollisionEvent<Player>>("Player", (state, p) => p.entity.ReceiveDamage(1))
                    .Event<CollisionEvent<Broom>>("Hit", (state, b) => GetHit())
                    .Exit((s) =>
                    {
                        s.exit_time = DateTime.Now;
                    })
                .End()
                .State<State>("Dying")
                    .Update((state, time) =>
                    {
                        opacity -= 0.05f;

                        if (opacity <= 0)
                        {
                            GlobalState.SpawnEntity(new Explosion(this));

                            exists = false;
                        }
                    })
                .End()
            .Build();

            _state.ChangeState("Pace");
        }

        public override void Update()
        {
            base.Update();

            _state.Update(GameTimes.DeltaTime);
        }

        protected override void AnimationChanged(string name)
        {
            base.AnimationChanged(name);

            if (name.EndsWith('l'))
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = SpriteEffects.None;
            }

            if (name.EndsWith('l') || name.EndsWith('r'))
            {
                shadow = _shadowH;

                width = 26;
                height = 13;

                offset = new Vector2(3, 11);
            }
            else
            {
                shadow = _shadowV;

                width = 10;
                height = 20;

                offset = new Vector2(10, 4);
            }

            shadow.visible = false;
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                _state.TriggerEvent("Player", new CollisionEvent<Player>() { entity = p });
            }
            else if (other is Broom b)
            {
                _state.TriggerEvent("Hit", new CollisionEvent<Broom>() { entity = b });
            }
        }

        private void GetHit()
        {
            if (_flickering)
            {
                return;
            }

            SoundManager.PlaySoundEffect("generic_hurt");

            Flicker(1);

            _health--;

            if (_health <= 0)
            {
                _state.ChangeState("Dying");
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return fireballs.Entities;
        }


            [Collision(typeof(Player), typeof(Broom), typeof(Dust), MapCollision = false)]
        class Fireball : Entity
        {
            private const int MaxDistance = 80;
            private const int ParentSize = 32;
            private const float FirebalVelocity = 88f;

            private Vector2 _startPos;

            public Fireball() : base(Vector2.Zero, "lion_fireballs", 16, 16, DrawOrder.FG_SPRITES)
            {
                width = height = 8;
                CenterOffset();

                AddAnimation("shoot", CreateAnimFrameArray(0, 1), 10);
                AddAnimation("poof", CreateAnimFrameArray(2, 3, 4, 5), 10);
            }

            public void Spawn(Entity parent, Entity target)
            {
                switch (parent.facing)
                {
                    case Facing.UP:
                        Position = new Vector2(parent.Position.X + ParentSize / 2, parent.Position.Y - 2);

                        velocity.X = GlobalState.RNG.Next(-16, 17);
                        velocity.Y = -ParentSize;
                        break;
                    case Facing.DOWN:
                        Position = new Vector2(parent.Position.X + ParentSize / 2, parent.Position.Y + parent.height + 2);

                        velocity.X = GlobalState.RNG.Next(-16, 17);
                        velocity.Y = ParentSize;
                        break;
                    case Facing.RIGHT:
                        Position = new Vector2(parent.Position.X + ParentSize, parent.Position.Y + 6);

                        velocity.X = ParentSize;
                        velocity.Y = GlobalState.RNG.Next(-26, 27);
                        break;
                    case Facing.LEFT:
                        Position = new Vector2(parent.Position.X, parent.Position.Y + 6);

                        velocity.X = -ParentSize;
                        velocity.Y = GlobalState.RNG.Next(-26, 27);
                        break;
                }

                velocity.Normalize();
                velocity *= FirebalVelocity;

                _startPos = Position;

                Play("shoot");
            }

            public override void Collided(Entity other)
            {
                if (CurAnimName == "poof")
                {
                    return;
                }

                if (other is Player p)
                {
                    p.ReceiveDamage(1);
                }

                Play("poof");
            }

            public override void Update()
            {
                if (_curAnim.Finished && CurAnimName == "poof")
                {
                    exists = false;

                    return;
                }

                if (Math.Abs(_startPos.X - Position.X) > MaxDistance || Math.Abs(_startPos.Y - Position.Y) > MaxDistance)
                {
                    Play("poof");
                }
            }
        }
    }
}
