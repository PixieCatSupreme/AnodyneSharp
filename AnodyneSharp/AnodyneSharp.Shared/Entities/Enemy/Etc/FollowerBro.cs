using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.FSM;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Etc
{
    [NamedEntity("Follower_Bro"), Collision(typeof(Player))]
    public class FollowerBro : Entity
    {
        private EntityPreset _preset;
        private Player _player;

        private IState _state;

        public static AnimatedSpriteRenderer GetSprite() => new("follower_bro", 16, 24,
            new Anim("wait", new int[] { 0 },1),
            new Anim("walk", new int[] { 1, 2, 1, 0 },4)
            );

        public FollowerBro(EntityPreset preset, Player p)
        : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;

            opacity = 0;

            _state = new StateMachineBuilder()
                .State("Hidden")
                    .Condition(() => _player.Position.Y - Position.Y < -20, (state) => state.Parent.ChangeState("Appearing"))
                .End()
                .State("Appearing")
                    .Enter((state) => { _player.dontMove = true; _preset.Alive = false; })
                    .Update((state, time) => opacity += 0.6f * time)
                    .Condition(() => opacity > 0.96f, (state) => state.Parent.ChangeState("Walk"))
                .End()
                .State("Walk")
                    .Enter((state) => { _player.dontMove = false; Play("walk"); })
                    .Update((state, time) => velocity = new Vector2((_player.Position.X > Position.X) ? 10 : -10, -10))
                    .Event<CollisionEvent<Player>>("Player", (state, _player) => state.Parent.ChangeState("Disappearing"))
                    .Condition(() => _player.Position.Y >= Position.Y, (state) => state.Parent.ChangeState("Wait"))
                .End()
                .State("Wait")
                    .Condition(() => _player.Position.Y < Position.Y, (state) => state.Parent.ChangeState("Walk"))
                .End()
                .State("Disappearing")
                    .Update((state, time) => opacity -= 0.6f * time)
                    .Condition(() => opacity <= 0, (state) => exists = false)
                .End()
                .Build();

            _state.ChangeState("Hidden");
        }

        public override void Update()
        {
            _state.Update(GameTimes.DeltaTime);
            base.Update();
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                _state.TriggerEvent("Player", new CollisionEvent<Player>() { entity = p });
            }
        }
    }
}
