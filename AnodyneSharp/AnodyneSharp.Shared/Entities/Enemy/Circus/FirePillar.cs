using AnodyneSharp.Entities.Enemy.Crowd;
using AnodyneSharp.FSM;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Circus
{
    [NamedEntity("Fire_Pillar"), Collision(typeof(Player), typeof(Dust), typeof(BaseSpikeRoller))]
    public class FirePillar : Entity
    {
        //Somehow the game starts lagging out when adding timers to every Enter State
        class IdleState : TimerState
        {
            public IdleState()
            {
                AddTimer(0.74f, "goToEmerge");
            }
        }
        class EmergeState : TimerState
        {
            public EmergeState()
            {
                AddTimer(0.3f, "goToFlame");
            }
        }
        class FlameState : TimerState
        {
            public FlameState()
            {
                AddTimer(1f, "goToRecede");
            }
        }
        class RecedeState : TimerState
        {
            public RecedeState()
            {
                AddTimer(0.3f, "goToIdle");
            }
        }

        IState _state;

        Entity _base;

        public FirePillar(EntityPreset preset, Player p)
            : base(preset.Position, "fire_pillar", 16, 32, Drawing.DrawOrder.ENTITIES)
        {

            _base = new Entity(preset.Position + new Vector2(0, 16), "fire_pillar_base", 16, 16, Drawing.DrawOrder.VERY_BG_ENTITIES);
            _base.AddAnimation("dormant", CreateAnimFrameArray(0, 1), 6);
            _base.Play("dormant");

            AddAnimation("idle", CreateAnimFrameArray(0), 15);
            AddAnimation("emerge", CreateAnimFrameArray(1, 2, 3, 4), 8, false);
            AddAnimation("flame", CreateAnimFrameArray(3, 4), 10);
            AddAnimation("recede", CreateAnimFrameArray(5, 6, 0), 8, false);
            Play("idle");

            height = 9;
            offset.Y += 16;
            Position.Y += 16;

            _state = new StateMachineBuilder()
                .State<IdleState>("Idle")
                    .Enter((s) =>
                    {
                        Play("idle");

                        visible = true;
                    })
                    .Event("goToEmerge", (s) =>
                    {
                        _state.ChangeState("Emerge");
                    })
                .End()
                .State<EmergeState>("Emerge")
                    .Enter((s) =>
                    {
                        Play("emerge");
                        Flicker(0.25f);
                    })
                    .Event("goToFlame", (s) =>
                    {
                        _state.ChangeState("Flame");
                    })
                .End()
                .State<FlameState>("Flame")
                    .Enter((s) =>
                    {
                        Play("flame");
                        SoundManager.PlaySoundEffect("flame_pillar");

                    })
                    .Event("goToRecede", (s) =>
                    {
                        _state.ChangeState("Recede");
                    })
                    .Event<CollisionEvent<Player>>("Player", (s, p) => p.entity.ReceiveDamage(1))
                .End()
                .State<RecedeState>("Recede")
                    .Enter((s) =>
                    {
                        Play("recede");
                    })
                    .Event("goToIdle", (s) =>
                    {
                        _state.ChangeState("Idle");
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
            else
            {
                _state.ChangeState("Idle");

                visible = false;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { _base };
        }
    }
}
