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
            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "BLANK", "intro", 0);


            _dialogueState = new DialogueState(isIntro: true);

            _state = new StateMachineBuilder()
                .State("Writing")
                    .Enter((state) => SoundManager.PlaySong("blank"))
                    .Update((state, t) => _dialogueState.Update())
                    .Condition(() => _dialogueState.Exit, (state) => _state.ChangeState("Wait"))
                .End()
                .State<WaitTimer>("Wait")
                    .Enter((state) => _dialogueState = null)
                    .Event("EndWait", (state) =>
                    {
                        Vector2 startPos = new Vector2(77, 87);

                        GlobalState.PLAYER_WARP_TARGET = startPos;
                        GlobalState.checkpoint = ("BLANK", startPos);

                        GlobalState.NEXT_MAP_NAME = "BLANK";

                        ChangeStateEvent(AnodyneGame.GameState.Game);
                    })
                .End()
            .Build();
        }

        public override void Create()
        {
            _dialogueState.Create();

            _state.ChangeState("Writing");
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
