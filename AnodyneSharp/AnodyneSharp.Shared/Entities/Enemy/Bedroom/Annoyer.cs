﻿using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Enemy.Circus;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Entities.Enemy
{
    //MapCollision conditionally enabled with Solid property
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom), MapCollision = true, KeepOnScreen = true)]
    public class Annoyer : Entity
    {
        public static AnimatedSpriteRenderer GetSprite(int frame)
        {
            int[] frames;
            int framerate = 4;
            if(GlobalState.IsCell)
            {
                frames = new int[] { 6, 7 };
            }
            else if(GlobalState.BoiEaster)
            {
                frames = new int[] { 8, 9 };
            }
            else
            {
                int i = frame == T_SUPER ? 12 : 0;
                frames = new int[] { i, i + 1, i + 2, i + 3, i + 4, i + 5 };
                framerate = 8;
            }
            return new("annoyer", 16, 16, new Anim("flap", frames, framerate));
        }

        private int _health = 1;

        private IState _state;

        private EntityPreset _preset;

        private Player _target;

        public const string DamageDealer = "Annoyer";
        public const string FireballDamageDealer = "Annoyer fireball";

        private const int T_SUPER = 2;

        private const float rotation_radius = 20f;
        private string start_state = "Wait";

        private Explosion explosion;
        private EntityPool<Fireball> fireballs;

        private class CirclingState : TimerState
        {
            public CirclingState()
            {
                AddTimer(3f, "Swoop");
            }
            public const float velocity = 8.4f;
            public float angle;

            public override void Update(float deltaTime)
            {
                angle += velocity * deltaTime;
                base.Update(deltaTime);
            }
        }

        private class ActiveState : TimerState
        {
            public ActiveState()
            {
                AddTimer(2.3f, "Fire");
            }
        }

        private class SwoopState : AbstractState
        {
            public Vector2 target;
        }

        private Vector2 ApproachTarget
        {
            get
            {
                return _target.Center + Vector2.UnitX * rotation_radius;
            }
        }

        public Annoyer(EntityPreset preset, Player player) 
            : base(preset.Position, GetSprite(preset.Frame), DrawOrder.ENTITIES)
        {
            MapInteraction = false;
            _preset = preset;
            _target = player;
            Solid = false;
            height = 7;
            width = 8;
            offset.X = 3;
            offset.Y = 2;

            explosion = new Explosion(Position)
            {
                exists = false
            };

            if (preset.Frame == T_SUPER)
                _health = 2;

            fireballs = new EntityPool<Fireball>(preset.Frame == T_SUPER ? 4 : 0, () => new Fireball());

            _state = new StateMachineBuilder()
                .State<ActiveState>("Active")
                    .Enter((state) => {
                        velocity = Vector2.Zero;
                        state.ChangeState(start_state);
                        start_state = "Approach"; //TODO: Add way of entering nested states(see ChangeState in Hit, needs to go to Approach, not Wait)
                    })
                    .Event<CollisionEvent<Player>>("Player", (state, p) => p.entity.ReceiveDamage(1, DamageDealer))
                    .Event<CollisionEvent<Broom>>("Hit", (state,b) => {
                        velocity = FacingDirection(b.entity.facing) * 150;
                        if (velocity.Y < 0)
                            velocity.X = GlobalState.RNG.Next(-30, 31);
                        _state.ChangeState("Hit");
                    })

                    .Event("Fire", (state) => fireballs.Spawn((f)=>f.Spawn(this,_target)))
                    
                    .State<TimerState>("Wait")
                        .Enter((state) =>
                        {
                            state.Reset();
                            state.AddTimer(0.25f, "ApproachCheck");
                        })
                        .Event("ApproachCheck", (state) =>
                        {
                            if ((Position - _target.Position).Length() < 64)
                                state.Parent.ChangeState("Approach");
                        })
                    .End()

                    .State("Approach")
                        .Update((state, time) => {
                            MathUtilities.MoveTo(ref Position.X, ApproachTarget.X, 36);
                            MathUtilities.MoveTo(ref Position.Y, ApproachTarget.Y, 36);
                        })
                        .Condition(() => (Position - ApproachTarget).Length() < 2, (state) => state.Parent.ChangeState("Circle"))
                        .Exit((state) => velocity = Vector2.Zero)
                    .End()
                    
                    .State<CirclingState>("Circle")
                        .Enter((state) => state.angle = 0)
                        .Update((state, time) =>
                        {
                            Position = _target.VisualCenter + new Vector2((float)Math.Cos(state.angle), (float)Math.Sin(state.angle)) * rotation_radius;
                        })
                        .Event("Swoop", (state) => state.Parent.ChangeState("Swoop"))
                    .End()
                    
                    .State<SwoopState>("Swoop")
                        .Enter((state) => state.target = Position + 3 * (_target.Position - Position) )
                        .Update((state,time) => { //TODO: make it possible for this to be a Condition
                            if (MathUtilities.MoveTo(ref Position.X, state.target.X, 2.5f*60) & MathUtilities.MoveTo(ref Position.Y, state.target.Y, 2.5f*60))
                                state.Parent.ChangeState("Approach");
                        })
                    .End()
                .End()

                .State<TimerState>("Hit")
                    .Enter((state) =>
                    {
                        SoundManager.PlaySoundEffect("player_hit_1");
                        state.Reset();
                        Flicker(0.2f);
                        if(--_health <= 0)
                        {
                            Solid = true;
                            state.AddTimer(0.25f, "Die");
                        }
                        else
                        {
                            state.AddTimer(0.4f, "EndKnockback");
                        }
                    })
                    .Event("EndKnockback", (state) => _state.ChangeState("Active"))
                    .Event("Die", (state) => {
                        exists = _preset.Alive = false;
                        explosion.exists = true;
                        explosion.Position = Position;
                        SoundManager.PlaySoundEffect("hit_wall");
                    })
                    .Exit((state) =>
                    {
                        Solid = false;
                    })
                .End()

                .Build();
            _state.ChangeState("Active");
        }

        public override void Update()
        {
            _state.Update(GameTimes.DeltaTime);
            base.Update();
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return fireballs.Entities.Concat(Enumerable.Repeat(explosion,1));
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

        public class Explosion : HealthDropper
        {
            public static AnimatedSpriteRenderer GetSprite(int o) => new AnimatedSpriteRenderer("enemy_explode_2", 24, 24, new Anim("explode", new int[] { o, o + 1, o + 2, o + 3, o + 4 }, GlobalState.IsCell ? 10 : 12, false));
            public Explosion(Vector2 pos) : base(null, pos, GetSprite(GlobalState.IsCell ? 5 : 0), DrawOrder.ENTITIES)
            {
            }

            public override void Update()
            {
                base.Update();
                if(AnimFinished)
                {
                    Die();
                }
            }
        }

        [Collision(typeof(Player),typeof(Broom),MapCollision = false)]
        public class Fireball : Entity
        {
            private const float speed = 30f;
            private IState _state;

            public Fireball() : base(Vector2.Zero, Lion.Fireball.GetSprite(8), DrawOrder.FG_SPRITES)
            {
                width = height = 8;
                offset = new Vector2(4, 4);

                _state = new StateMachineBuilder()
                    .State("Shoot")
                        .Enter((state) => Play("shoot"))
                        .Update((state,time) => opacity -= 0.06f * time)
                        .Condition(()=>opacity <= 0.6f, (s) => _state.ChangeState("Poof"))
                        .Event<CollisionEvent<Broom>>("Hit",(s,b) => _state.ChangeState("Poof"))
                        .Event<CollisionEvent<Player>>("Player",(s,p) => { p.entity.ReceiveDamage(1, FireballDamageDealer); _state.ChangeState("Poof"); })
                    .End()
                    .State("Poof")
                        .Enter((state) => Play("poof"))
                        .Condition(()=> AnimFinished, (s) => exists=false)
                    .End()
                    .Build();
            }

            public void Spawn(Entity parent, Entity target)
            {
                Position = parent.Position;
                MoveTowards(target.Position, speed);
                opacity = 1.0f;
                _state.ChangeState("Shoot");
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

            public override void Update()
            {
                _state.Update(GameTimes.DeltaTime);
                base.Update();
            }
        }
    }
}
