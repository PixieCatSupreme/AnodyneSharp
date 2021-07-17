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
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Redcave
{
    [NamedEntity, Collision(typeof(Broom), KeepOnScreen = true)]
    public class Slasher : HealthDropper
    {
        private const int WIDE_ATK_DISTANCE = 36;

        private IState _state;
        private Facing _oldFacing;

        Player _player;
        EntityPreset _preset;

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

            _player = p;
            _preset = preset;

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
                        float dx = (_player.Position.X + 8) - (Position.X + width / 2);
                        float dy = (_player.Position.Y + 8) - (Position.Y + height / 2);

                        if (Math.Sqrt(dx * dx + dy * dy) < WIDE_ATK_DISTANCE)
                        {
                            _state.ChangeState("WideAttack");

                            //update_wide_pos();
                        }
                        else
                        {
                            _state.ChangeState("LongAttack");

                            //update_long_pos();
                        }

                    })
                .End()
                .State<TimerState>("WideAttack")
                    .Enter((state) =>
                    {
                        state.Reset();

                        SoundManager.PlaySoundEffect("slasher_atk");

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

        [Collision(typeof(Player))]
        private class Slash : Entity
        {
            public Slash(bool isWide)
                : base(Vector2.Zero, isWide ? "f_slasher_wide" : "f_slasher_long", isWide ? 48 : 16, isWide ? 16 : 48, Drawing.DrawOrder.ENTITIES)
            {
                if (isWide)
                {
                    AddAnimation("slash", CreateAnimFrameArray(0, 1, 2, 6), 12, false);
                }
                else
                {
                    AddAnimation("slash", CreateAnimFrameArray(3, 4, 5, 6), 12, false);
                }
            }
        }
    }
}
