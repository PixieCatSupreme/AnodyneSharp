using AnodyneSharp.Dialogue;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States
{
    public class IntroState : State
    {
        class WaitTimer : TimerState
        {
            public WaitTimer()
            {
                AddTimer(1.5f, "EndWait");
            }
        }

        DialogueState _dialogueState;

        private IState _state;

        public IntroState()
        {
            string diag = DialogueManager.GetDialogue("sage", "BLANK", "intro", 0);

            _state = new StateMachineBuilder()
                .State<WaitTimer>("WaitStart")
                    .Enter((state) => SoundManager.PlaySong("blank"))
                    .Event("EndWait", (state) => _state.ChangeState("Writing"))
                .End()
                .State("Writing")
                    .Enter((state) =>
                    {
                        _dialogueState = new DialogueState(diag,isIntro: true);
                        _dialogueState.Create();
                    })
                    .Update((state, t) => _dialogueState.Update())
                    .Condition(() => _dialogueState.Exit, (state) => _state.ChangeState("WaitEnd"))
                .End()
                .State<WaitTimer>("WaitEnd")
                    .Enter((state) => _dialogueState = null)
                    .Event("EndWait", (state) =>
                    {
                        Vector2 startPos = new Vector2(77, 87);

                        GlobalState.PLAYER_WARP_TARGET = startPos;
                        GlobalState.checkpoint = new GlobalState.CheckPoint("BLANK", startPos);

                        GlobalState.NEXT_MAP_NAME = "BLANK";

                        GlobalState.events.ActivatedNexusPortals.Add("STREET");

                        ChangeStateEvent(AnodyneGame.GameState.Game);
                    })
                .End()
            .Build();

            _state.ChangeState("WaitStart");
        }

        public override void Update()
        {
            _state.Update(GameTimes.DeltaTime);
        }

        public override void DrawUI()
        {
            if (_dialogueState != null)
            {
                _dialogueState.DrawUI();
            }
        }
    }
}
