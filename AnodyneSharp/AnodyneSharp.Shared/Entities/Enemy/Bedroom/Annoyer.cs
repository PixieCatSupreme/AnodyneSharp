using AnodyneSharp.Drawing;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy
{
    //MapCollision conditionally enabled with Solid property
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom), MapCollision = true)]
    public class Annoyer : Entity
    {
        private int _health = 1;

        private IState _state;

        private EntityPreset _preset;

        private Player _target;

        private const int T_SUPER = 2;

        private const float rotation_radius = 20f;

        private string start_state = "Wait";

        private Explosion explosion;

        private class CirclingState : TimerState
        {
            public CirclingState()
            {
                AddTimer(3f, "Swoop");
            }
            public const float velocity = 4.25f/3f*6.28f; //4 1/4 rotations in 3 seconds
            public float angle;

            public override void Update(float deltaTime)
            {
                angle += velocity * deltaTime;
                base.Update(deltaTime);
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
            : base(preset.Position, "annoyer", 16,16, DrawOrder.ENTITIES)
        {
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

            if (GlobalState.CURRENT_MAP_NAME == "TRAIN")
            {
                AddAnimation("flap", CreateAnimFrameArray(6, 7), 4, true);
            }
            else
            {
                int i = preset.Frame == T_SUPER ? 12 : 0;
                AddAnimation("flap", CreateAnimFrameArray(i,i+1,i+2,i+3,i+4,i+5), 8, true);
            }
            Play("flap");

            if (preset.Frame == T_SUPER)
                _health = 2;

            _state = new StateMachineBuilder()
                .State("Active")
                    .Enter((state) => {
                        velocity = Vector2.Zero;
                        state.ChangeState(start_state);
                        start_state = "Approach"; //TODO: Add way of entering nested states(see ChangeState in Hit, needs to go to Approach, not Wait)
                    })
                    .Event<CollisionEvent<Player>>("Player", (state, p) => p.entity.ReceiveDamage(1))
                    .Event<CollisionEvent<Broom>>("Hit", (state,b) => {
                        velocity = FacingDirection(b.entity.facing) * 150;
                        if (velocity.Y < 0)
                            velocity.X = GlobalState.RNG.Next(-30, 31);
                        _state.ChangeState("Hit");
                    })
                    
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
                        .Update((state, time) => MoveTowards(ApproachTarget, 30f))
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
                        .Enter((state) => { state.target = Position + 3 * (_target.Position - Position); MoveTowards(state.target, 100f); })
                        .Update((state,time) => { //TODO: make it possible for this to be a Condition
                            if ((Position - state.target).Length() < 3)
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

        private void MoveTowards(Vector2 target, float speed)
        {
            velocity = target - Position;
            velocity.Normalize();
            velocity *= speed;
        }

        public override void Update()
        {
            _state.Update(GameTimes.DeltaTime);
            base.Update();
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(explosion,1);
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

        class Explosion : Entity
        {
            public Explosion(Vector2 pos) : base(pos, "enemy_explode_2", 24, 24, DrawOrder.ENTITIES)
            {
                AddAnimation("explode", CreateAnimFrameArray(0, 1, 2, 3, 4), 12, false);
                Play("explode");
            }

            public override void Update()
            {
                base.Update();
                if(finished)
                {
                    //TODO: drop health pickup
                    exists = false;
                }
            }
        }
    }
}
