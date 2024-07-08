using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Redcave
{
    [NamedEntity, Enemy, Collision(typeof(Broom), KeepOnScreen = true, MapCollision = true)]
    public class Slasher : HealthDropper
    {
        private const int WIDE_ATK_DISTANCE = 36;

        public const string WideDamageDealer = "Slasher wide attack";
        public const string LongDamageDealer = "Slasher long attack";

        private IState _state;
        private Facing _oldFacing;

        Player _player;

        VerticalSlash _vertical;
        HorizontalSlash _horizontal;

        int _health = 3;

        public static AnimatedSpriteRenderer GetSprite() => new("f_slasher", 24, 24,
            new Anim("float_d", new int[] { 0, 1 }, 3),
            new Anim("float_l", new int[] { 2, 3 }, 3),
            new Anim("float_u", new int[] { 4, 5 }, 3),
            new Anim("warning_d", new int[] { 6, 6 }, 10, false), //Duplicate the last frame
            new Anim("warning_l", new int[] { 7, 7 }, 10, false),
            new Anim("warning_u", new int[] { 8, 8 }, 10, false),
            new Anim("attack_d", new int[] { 9, 9 }, 10, false),
            new Anim("attack_l", new int[] { 10, 10 }, 10, false),
            new Anim("attack_u", new int[] { 11, 11 }, 10, false),
            new Anim("die", new int[] { 9, 10, 11, 11 }, 3, false)
            );

        public Slasher(EntityPreset preset, Player p)
            : base(preset, preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES, 1, true)
        {
            
            width = height = 16;

            MapInteraction = false;

            _player = p;

            _vertical = new();
            _horizontal = new();

            CenterOffset();

            _state = new StateMachineBuilder()
                .State<TimerState>("Move")
                    .Enter((state) =>
                    {
                        state.Reset();

                        PlayFacingFix("float");

                        state.AddTimer(1.4f, "MoveTimer");
                    })
                    .Update((state, t) =>
                    {
                        Move();

                        if (_oldFacing != facing)
                        {
                            PlayFacingFix("float");
                        }
                    })
                    .Event("MoveTimer", (state) =>
                    {
                        _state.ChangeState("Warning");
                    })
                .End()
                .State<TimerState>("Warning")
                    .Enter((state) =>
                    {
                        state.Reset();

                        PlayFacingFix("warning");

                        state.AddTimer(0.5f, "WarningTimer");
                    })
                    .Update((state, t) =>
                    {
                        Move();

                        if (_oldFacing != facing)
                        {
                            PlayFacingFix("warning");
                        }
                    })
                    .Event("WarningTimer", (state) =>
                    {
                        if ((Center - _player.VisualCenter).LengthSquared() < WIDE_ATK_DISTANCE * WIDE_ATK_DISTANCE)
                        {
                            _state.ChangeState("WideAttack");
                        }
                        else
                        {
                            _state.ChangeState("LongAttack");
                        }

                    })
                .End()
                .State<TimerState>("WideAttack")
                    .Enter((state) =>
                    {
                        state.Reset();

                        SoundManager.PlaySoundEffect("slasher_atk");

                        if (facing == Facing.UP || facing == Facing.DOWN)
                        {
                            _horizontal.UpdateWidePosVertical(Position, facing);
                        }
                        else
                        {
                            _vertical.UpdateWidePosHorizontal(Position, facing);
                        }

                        PlayFacingFix("attack");

                        state.AddTimer(0.4f, "AttackTimer");
                    })
                    .Event("AttackTimer", (state) =>
                    {
                        _state.ChangeState("WideAttackDelay");
                    })
                .End()
                .State<TimerState>("WideAttackDelay")
                    .Enter((state) =>
                    {
                        state.Reset();

                        state.AddTimer(0.3f, "DelayTimer");
                    })
                    .Event("DelayTimer", (state) =>
                    {
                        _state.ChangeState("Move");
                    })
                .End()
                .State<TimerState>("LongAttack")
                    .Enter((state) =>
                    {
                        state.Reset();

                        SoundManager.PlaySoundEffect("slasher_atk");


                        if (facing == Facing.UP || facing == Facing.DOWN)
                        {
                            _vertical.UpdateLongPosVertical(Position, facing);
                        }
                        else
                        {
                            _horizontal.UpdateLongPosHorizontal(Position, facing);
                        }

                        PlayFacingFix("attack");

                        state.AddTimer(0.8f, "AttackTimer");
                    })
                    .Event("AttackTimer", (state) =>
                    {
                        _state.ChangeState("Move");
                    })
                .End()
            .Build();

            _state.ChangeState("Move");
        }

        public override void Update()
        {
            base.Update();

            _state.Update(GameTimes.DeltaTime);
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (!_flickering)
            {
                _health--;
                Flicker(0.5f);

                if (_health <= 0)
                {
                    Die();

                    GlobalState.SpawnEntity(new Explosion(this));
                }
            }
        }

        private void Move()
        {
            MathUtilities.MoveTo(ref Position.X, _player.Position.X, 20);
            MathUtilities.MoveTo(ref Position.Y, _player.Position.Y, 20);

            _oldFacing = facing;

            FaceTowards(_player.Position);
        }

        private void PlayFacingFix(string animName)
        {
            if (facing == Facing.RIGHT)
            {
                Play($"{animName}_l");

                _flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                PlayFacing(animName);

                _flip = SpriteEffects.None;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(new List<Entity>() { _horizontal, _vertical });
        }

        [Collision(typeof(Player))]
        public class Slash : Entity
        {
            string DamageType;

            public Slash(Vector2 pos, AnimatedSpriteRenderer sprite, DrawOrder layer)
                : base(pos, sprite, layer)
            {
                exists = false;
            }

            public override void Update()
            {
                base.Update();

                if (AnimFinished)
                {
                    exists = false;
                }
            }

            protected void Reset(Vector2 pos, string damageType)
            {
                DamageType = damageType;
                exists = true;

                Position = pos;

                offset = Vector2.Zero;

                _flip = SpriteEffects.None;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);

                if (visible && other is Player p)
                {
                    p.ReceiveDamage(1, DamageType);
                }
            }
        }

        public class HorizontalSlash : Slash
        {
            public static AnimatedSpriteRenderer GetSprite() => new("f_slasher_wide", 48, 16,
                new Anim("wide", new int[] { 0, 1, 2 },12,false),
                new Anim("tall", new int[] { 3, 4, 5 },12,false)
                );

            public HorizontalSlash()
                : base(Vector2.Zero, GetSprite(), DrawOrder.ENTITIES)
            {
                width = 36;
                height = 10;
            }

            public void UpdateLongPosHorizontal(Vector2 pos, Facing facing)
            {
                Reset(pos,LongDamageDealer);

                Position.Y += -height / 2 + 8;

                offset.Y = 4;

                Play("tall");

                if (facing == Facing.RIGHT)
                {
                    Position.X += 16;

                    offset.X = 4;
                }
                else
                {
                    _flip = SpriteEffects.FlipHorizontally;
                    Position.X -= 36;

                    offset.X = 11;
                    offset.Y -= 1;
                }
            }

            public void UpdateWidePosVertical(Vector2 pos, Facing facing)
            {
                Reset(pos, WideDamageDealer);

                Position.X += -width / 2 + 8;

                offset.X = 6;

                Play("wide");

                if (facing == Facing.UP)
                {
                    _flip = SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;

                    Position.Y -= 4;
                    offset.Y = 4;

                    offset.X -= 1;
                }
                else
                {
                    Position.Y += 10;

                    offset.Y = 6;
                }
            }
        }

        public class VerticalSlash : Slash
        {
            public static AnimatedSpriteRenderer GetSprite() => new("f_slasher_long", 16, 48,
                new Anim("wide", new int[] { 0, 1, 2 }, 12, false),
                new Anim("tall", new int[] { 3, 4, 5 }, 12, false)
                );

            public VerticalSlash()
                : base(Vector2.Zero, GetSprite(), DrawOrder.ENTITIES)
            {
                width = 10;
                height = 36;
            }

            //For long up and down
            public void UpdateLongPosVertical(Vector2 pos, Facing facing)
            {
                Reset(pos, LongDamageDealer);

                Position.X += -width / 2 + 8;

                offset.X = 2;

                Play("tall");

                if (facing == Facing.UP)
                {
                    Position.Y -= 36;
                    offset.Y = 10;

                    offset.X += 1;
                }
                else
                {
                    _flip = SpriteEffects.FlipVertically;
                    Position.Y += 16;

                    offset.Y = 3;
                }
            }

            public void UpdateWidePosHorizontal(Vector2 pos, Facing facing)
            {
                Reset(pos, WideDamageDealer);

                Position.Y += -height / 2 + 8;

                offset.Y = 6;

                Play("wide");

                if (facing == Facing.RIGHT)
                {
                    Position.X += 11;
                }
                else
                {
                    _flip = SpriteEffects.FlipHorizontally;

                    Position.X -= 5;

                    offset.X = 6;
                }
            }
        }
    }
}
