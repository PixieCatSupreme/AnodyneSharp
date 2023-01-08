using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
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

        public static AnimatedSpriteRenderer GetSprite(int frame)
        {
            int[] shooter_closed = frame == 3 || frame == 4 ? new int[] { 3, 4 } : new int[] { 0, 1 };
            int[] shooter_open = frame == 3 || frame == 4 ? new int[] { 5 } : new int[] { 2 };

            return new("on_off_shooter", 16, 16, new Anim("shooter_closed", shooter_closed, 2), new Anim("shooter_open", shooter_open, 1));
        }

        public OnOffLaser(EntityPreset preset, Player player)
            : base(preset.Position, GetSprite(preset.Frame), DrawOrder.ENTITIES)
        {
            immovable = true;

            facing = (preset.Frame) switch
            {
                1 => Facing.RIGHT,
                2 => Facing.DOWN,
                3 => Facing.LEFT,
                _ => Facing.UP,
            };

            rotation = facing == Facing.RIGHT || facing == Facing.LEFT ? MathHelper.ToRadians(-90) : 0;            

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
            public static AnimatedSpriteRenderer GetSprite(bool horiz) => new(horiz ? "f_on_off_h" : "f_on_off_v", horiz ? 144 : 16, horiz ? 16 : 144,
                new Anim("idle", new int[] { 0 },1),
                new Anim("steam_is_harmful",new int[] { 3, 4, 5, 6 }, 15),
                new Anim("recede", new int[] { 0, 1, 2 },5,false),
                new Anim("emerge", new int[] { 2, 1, 0 },5,false)
                );

            public Laser(Vector2 pos, Facing direction)
                : base(pos, GetSprite(direction == Facing.LEFT || direction == Facing.RIGHT), DrawOrder.ENTITIES)
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

                        width = 8;
                        height = 112;

                        Position += new Vector2(4, 16);
                        offset = new Vector2(4, 16);
                        break;
                }

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
