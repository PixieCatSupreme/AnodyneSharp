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
            public IEnumerator<string> state;
            public OpenState()
            {
                AddTimer(0.8f, "open");
            }
        }

        protected IState _state;
        protected Player _player;

        public BigGate(EntityPreset preset, Player p) : base(preset, "gate_green", 32, 16, Drawing.DrawOrder.ENTITIES)
        {
            _sentinel.Position = Position + new Vector2(11, -3);
            _sentinel.width = 10;
            _player = p;

            _state = new StateMachineBuilder()
                .State<OpenState>("Open")
                    .Enter(s =>
                    {
                        s.state = openSequence();

                        IEnumerator<string> openSequence()
                        {
                            yield return "Start";
                            for(int i = 0; i <= 4; ++i)
                            {
                                GlobalState.screenShake.Shake(0.02f, 0.3f);
                                SoundManager.PlaySoundEffect("hit_ground_1");
                                SetFrame(i);
                                yield return $"Open {i}";
                            }
                            SoundManager.PlaySoundEffect("open");
                            _player.state = PlayerState.GROUND;
                            exists = false;
                            yield break;
                        }
                    })
                    .Event("open", (s) =>
                    {
                        s.state.MoveNext();
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
