using AnodyneSharp.Drawing;
using AnodyneSharp.FSM;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Redcave
{
    [NamedEntity("On_Off_Laser")]
    public class OnOffLaser : Entity
    {
        private IState _state;
        private Laser _laser;

        public OnOffLaser(EntityPreset preset, Player player)
            : base(preset.Position, "on_off_shooter", 16, 16, DrawOrder.ENTITIES)
        {
            int[] shooter_closed;
            int[] shooter_open;

            immovable = true;

            facing = (preset.Frame) switch
            {
                1 => Facing.RIGHT,
                2 => Facing.DOWN,
                3 => Facing.LEFT,
                _ => Facing.UP,
            };

            switch (facing)
            {
                case Facing.LEFT:
                    rotation = MathHelper.ToRadians(-90);

                    shooter_closed = CreateAnimFrameArray(3, 4);
                    shooter_open = CreateAnimFrameArray(5);
                    break;
                case Facing.UP:
                    shooter_closed = CreateAnimFrameArray(3, 4);
                    shooter_open = CreateAnimFrameArray(5);
                    break;
                case Facing.RIGHT:
                    rotation = MathHelper.ToRadians(-90);

                    shooter_closed = CreateAnimFrameArray(0, 1);
                    shooter_open = CreateAnimFrameArray(2);
                    break;
                default:
                    shooter_closed = CreateAnimFrameArray(0, 1);
                    shooter_open = CreateAnimFrameArray(2);
                    break;
            }

            AddAnimation("shooter_closed", shooter_closed, 2);
            AddAnimation("shooter_open", shooter_open);

            

            _laser = new Laser(Position, facing);

            _state = new StateMachineBuilder()
                .State<TimerState>("Idle")
                    .Enter((state) =>
                    {
                        state.Reset();

                        Play("shooter_closed");
                        state.AddTimer(2f, "StartTimer");
                    })
                    .Event("StartTimer", (state) =>
                    {
                        _state.ChangeState("Emerging");
                    })
                .End()
                .State("Emerging")
                    .Enter((state) =>
                    {
                        Play("shooter_open");

                        SoundManager.PlaySoundEffect("on_off_laser_shoot");

                        _laser.visible = true;
                        _laser.Play("emerge");

                        _laser.Flicker(0.55f);
                    })
                    .Condition(_laser.EndofAnim, (s) => _state.ChangeState("Hurting"))
                .End()
                .State<TimerState>("Hurting")
                    .Enter((state) =>
                    {
                        state.Reset();

                        _laser.Play("steam_is_harmful");

                        state.AddTimer(2f, "HurtTimer");
                    })
                    .Event("HurtTimer", (state) =>
                    {
                        _state.ChangeState("Receding");

                    })
                .End()
                .State("Receding")
                    .Enter((state) =>
                    {
                        _laser.Play("recede");

                        _laser.Flicker(0.55f);
                    })
                    .Condition(_laser.EndofAnim, (s) => _state.ChangeState("Idle"))
                    .Exit((state) =>
                    {
                        _laser.visible = false;
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

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(_laser, 1);
        }

        [Collision(typeof(Player), typeof(Mover))]
        class Laser : Entity
        {
            public Laser(Vector2 pos, Facing direction)
                : base(pos, "f_on_off_h", 144, 16, DrawOrder.ENTITIES)
            {
                switch (direction)
                {
                    case Facing.LEFT:
                        width = 112;
                        height = 8;

                        Position += new Vector2(-width, 4);
                        offset = new Vector2(16, 4);
                        break;
                    case Facing.UP:
                        SetTexture("f_on_off_v", 16, 144);

                        _flip = SpriteEffects.FlipVertically;

                        width = 8;
                        height = 112;

                        Position += new Vector2(4, -height);
                        offset = new Vector2(4, 16);
                        break;
                    case Facing.RIGHT:
                        _flip = SpriteEffects.FlipHorizontally;

                        width = 128;
                        height = 8;

                        Position += new Vector2(4, 4);
                        offset = new Vector2(4, 4);
                        break;
                    default:
                    case Facing.DOWN:
                        SetTexture("f_on_off_v", 16, 144);

                        width = 8;
                        height = 112;

                        Position += new Vector2(4, 16);
                        offset = new Vector2(4, 16);
                        break;
                }

                AddAnimation("steam_is_harmful", CreateAnimFrameArray(3, 4, 5, 6), 15, true);
                AddAnimation("recede", CreateAnimFrameArray(0, 1, 2), 5, false);
                AddAnimation("emerge", CreateAnimFrameArray(2, 1, 0), 5, false);

                SetFrame(0);

                visible = false;
            }

            public override void Collided(Entity other)
            {
                if (CurAnimName == "steam_is_harmful")
                {
                    if (other is Player p)
                    {
                        p.ReceiveDamage(1);
                    }
                    else if (other is Mover m)
                    {
                        m.Die();
                    }
                }
            }

            internal bool EndofAnim()
            {
                return AnimFinished;
            }
        }
    }
}
