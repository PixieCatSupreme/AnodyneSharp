using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    public abstract class BigGate : KeyCardGate
    {
        private class OpenState : TimerState
        {
            public OpenState()
            {
                AddTimer(0.8f, "open");
            }
        }

        protected IState _state;
        protected Player _player;

        public BigGate(EntityPreset preset, Player p) : base(preset, "gate_green", 32, 16, Drawing.DrawOrder.ENTITIES)
        {
            _sentinel.Position = Position + new Vector2(6, -3);
            _sentinel.width = 10;
            _player = p;

            _state = new StateMachineBuilder()
                .State<OpenState>("Open")
                    .Event("open", (s) =>
                    {
                        if (_curAnim.Frame != 4)
                        {
                            GlobalState.screenShake.Shake(0.02f, 0.3f);
                            SoundManager.PlaySoundEffect("hit_ground_1");
                        }
                        switch (_curAnim.Frame)
                        {
                            case < 4:
                                SetFrame(_curAnim.Frame + 1);
                                break;
                            case > 4:
                                SetFrame(0);
                                break;
                            case 4:
                                SoundManager.PlaySoundEffect("open");
                                _player.state = PlayerState.GROUND;
                                exists = false;
                                break;
                        }
                    })
                .End()
                .Build();
        }

        public override void Update()
        {
            base.Update();
            _state.Update(GameTimes.DeltaTime);
        }
    }
}
