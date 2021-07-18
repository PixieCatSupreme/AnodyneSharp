using AnodyneSharp.Drawing;
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
    [NamedEntity, Collision(typeof(Broom), KeepOnScreen = true, MapCollision = true)]
    public class Slasher : HealthDropper
    {
        private const int WIDE_ATK_DISTANCE = 36;

        private IState _state;
        private Facing _oldFacing;

        Player _player;
        EntityPreset _preset;

        VerticalSlash _vertical;
        HorizontalSlash _horizontal;

        int _health = 3;

        public Slasher(EntityPreset preset, Player p)
            : base(preset.Position, "f_slasher", 24, 24, Drawing.DrawOrder.ENTITIES, 1, true)
        {
            AddAnimation("float_d", CreateAnimFrameArray(0, 1), 3);
            AddAnimation("float_l", CreateAnimFrameArray(2, 3), 3);
            AddAnimation("float_u", CreateAnimFrameArray(4, 5), 3);
            AddAnimation("warning_d", CreateAnimFrameArray(6, 6), 10, false); //Duplicate the last frame
            AddAnimation("warning_l", CreateAnimFrameArray(7, 7), 10, false);
            AddAnimation("warning_u", CreateAnimFrameArray(8, 8), 10, false);
            AddAnimation("attack_d", CreateAnimFrameArray(9, 9), 10, false);
            AddAnimation("attack_l", CreateAnimFrameArray(10, 10), 10, false);
            AddAnimation("attack_u", CreateAnimFrameArray(11, 11), 10, false);
            AddAnimation("die", CreateAnimFrameArray(9, 10, 11, 11), 3, false);

            Play("float_d");

            width = height = 16;

            MapInteraction = false;

            _player = p;
            _preset = preset;

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
                    _preset.Alive = false;

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
        private class Slash : Entity
        {
            public Slash(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer)
                : base(pos, textureName, frameWidth, frameHeight, layer)
            {
                exists = false;
            }

            public override void Update()
            {
                base.Update();

                if (_curAnim.Finished)
                {
                    exists = false;
                }
            }

            protected void Reset(Vector2 pos)
            {
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
                    p.ReceiveDamage(1);
                }
            }
        }

        private class HorizontalSlash : Slash
        {
            public HorizontalSlash()
                : base(Vector2.Zero, "f_slasher_wide", 48, 16, DrawOrder.ENTITIES)
            {
                AddAnimation("wide", CreateAnimFrameArray(0, 1, 2), 12, false);
                AddAnimation("tall", CreateAnimFrameArray(3, 4, 5), 12, false);

                width = 36;
                height = 10;
            }

            public void UpdateLongPosHorizontal(Vector2 pos, Facing facing)
            {
                Reset(pos);

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
                Reset(pos);

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

        private class VerticalSlash : Slash
        {
            public VerticalSlash()
                : base(Vector2.Zero, "f_slasher_long", 16, 48, DrawOrder.ENTITIES)
            {

                AddAnimation("wide", CreateAnimFrameArray(0, 1, 2), 12, false);
                AddAnimation("tall", CreateAnimFrameArray(3, 4, 5), 12, false);

                width = 10;
                height = 36;
            }

            //For long up and down
            public void UpdateLongPosVertical(Vector2 pos, Facing facing)
            {
                Reset(pos);

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
                Reset(pos);

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
