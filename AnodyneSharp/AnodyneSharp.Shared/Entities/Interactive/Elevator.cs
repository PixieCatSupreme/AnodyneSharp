using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive
{
    [NamedEntity]
    public class Elevator : Entity
    {
        EntityPreset _preset;

        PlayerDetector openDetector;
        PlayerDetector menuDetector;

        Player _player;

        IState _state;

        public Elevator(EntityPreset preset, Player p) : base(preset.Position,"elevator",32,32,Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;

            openDetector = new(new((Position + Vector2.UnitY * 32).ToPoint(), new(width, height)));
            menuDetector = new(new(Position.ToPoint(), new(32, 20)));

            AddAnimation("open",  CreateAnimFrameArray(0, 1, 2, 3), 12, false);
            AddAnimation("close", CreateAnimFrameArray(3, 2, 1, 0), 12, false);
            SetFrame(0);

            _state = new StateMachineBuilder()
                .State("Init")
                    .Condition(()=>openDetector.Hit,(s)=>_state.ChangeState("Open"))
                .End()
                .State("Close")
                    .Enter((s)=> { Play("close"); SoundManager.PlaySoundEffect("elevator_close"); })
                    .Condition(()=>openDetector.Hit,(s)=>_state.ChangeState("Open"))
                    .Condition(()=>menuDetector.Hit,(s)=>_state.ChangeState("Menu"))
                .End()
                .State("Open")
                    .Enter((s)=> { Play("open"); SoundManager.PlaySoundEffect("elevator_open"); })
                    .Condition(()=>!openDetector.Hit,(s)=>_state.ChangeState("Close"))
                .End()
                .State("Menu")
                    .Enter((s)=>DoMenu()) //for now just warp
                .End()
            .Build();
            _state.ChangeState("Init");
        }

        void DoMenu()
        {
            var others = EntityManager.GetLinkGroup(_preset.LinkID).Where(e => e.LinkID != _preset.LinkID).ToLookup(e => e.Frame);
            EntityPreset next = others[(_preset.Frame + 1)%(others.Count+1)].First();
            GlobalState.PLAYER_WARP_TARGET = next.Position + new Vector2(8, 30);
            GlobalState.NEXT_MAP_NAME = GlobalState.CURRENT_MAP_NAME;
            GlobalState.WARP = true;
        }

        public override void Update()
        {
            base.Update();
            _state.Update(GameTimes.DeltaTime);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { menuDetector, openDetector };
        }

        [Collision(typeof(Player))]
        class PlayerDetector : Entity
        {
            public bool Hit = false;

            public PlayerDetector(Rectangle r) : base(new(r.X,r.Y),r.Width,r.Height,Drawing.DrawOrder.BACKGROUND)
            {
                visible = false;
            }

            public override void Update()
            {
                base.Update();
                Hit = false;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                Hit = true;
            }
        }
    }
}
