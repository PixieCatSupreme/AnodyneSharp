using AnodyneSharp.FSM;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.Font;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RSG;

namespace AnodyneSharp.States
{
    public class DialogueState : State
    {

        private class BumpState : TimerState
        {
            public int halfBumps = 0;
            public int linesBumped = 0;

            public BumpState()
            {
                AddTimer(0.2f, "doBump");
            }
        }

        private TextBox _tb;

        private IState _state;

        private int normalSpeed;

        public DialogueState(bool useMenuBox = false)
        {
            _tb = new TextBox(useMenuBox);
            _tb.Writer.SetSpriteFont(FontManager.InitFont(Color.White), ResourceManager.GetTexture("consoleButtons"));
            _tb.Writer.Text = GlobalState.Dialogue;
            _tb.Writer.DrawShadow = true;

            normalSpeed = _tb.Writer.Speed;

            /*
             * Standard stack:
             * 
             *     Waiting, Writing
             *   =>Waiting
             *   =>Bump
             *   =>Bump, Writing
             *   =>Bump
             *   =>...
             *   =>Bump
             *   =>Waiting
             *   =>Waiting,Writing
             *   =>Waiting
             *   =>...
             *   =>Waiting
             *   =>Done
             */

            _state = new StateMachineBuilder()
                .State("Writing")
                    //Make sure writing only happens in this state
                    .Enter((state) => _tb.PauseWriting = false)
                    .Exit((state) => _tb.PauseWriting = true)

                    .Condition(() => _tb.Writer.JustWrittenChar, (state) =>
                    {
                        SoundManager.PlaySoundEffect("dialogue_blip");
                    })
                    .Condition(() => _tb.Writer.AtEndOfText || _tb.Writer.AtEndOfBox || _tb.Writer.NextCharacter == '^', (state) =>
                      {
                          if (_tb.Writer.NextCharacter == '^')
                          {
                              _tb.Writer.SkipCharacter();
                              _state.ChangeState("WaitCaret");
                              return;
                          }
                          _state.PopState();
                          if(_tb.Writer.AtEndOfText)
                          {
                              _state.ChangeState("Waiting");
                          }
                      })
                .End()
                .State("WaitCaret")
                    .Event("KeyPressed",(state) =>
                    {
                        SoundManager.PlaySoundEffect("dialogue_bloop");
                        _state.ChangeState("Writing");
                    })
                .End()
                .State<BumpState>("Bump")
                    .Enter((s) => s.linesBumped = 0)
                    .Event("doBump", (state) =>
                     {
                         if (state.halfBumps == 0)
                         {
                             _tb.Writer.RemoveFirstLine();
                         }

                         if (state.halfBumps < 2)
                         {
                             state.halfBumps++;
                             _tb.Writer.PushTextUp();
                         }
                         else
                         {
                             state.halfBumps = 0;
                             state.linesBumped++;
                             if (state.linesBumped == _tb.Writer.LinesPerBox) //Bumped all lines, move back to Waiting
                                 _state.ChangeState("Waiting");
                             _state.PushState("Writing");
                         }
                     })
                .End()
                .State("Waiting")
                    .Event("KeyPressed", (state) =>
                    {
                        SoundManager.PlaySoundEffect("dialogue_bloop");

                        if (_tb.Writer.AtEndOfText)
                        {
                            _state.ChangeState("Done");
                        }
                        else if (_tb.Writer.AtEndOfBox)
                        {
                            _state.ChangeState("Bump");
                            _state.TriggerEvent("doBump");
                        }
                        else
                        {
                            _state.PushState("Writing");
                        }
                    })
                .End()
                .State("Done")
                    .Enter((state) => { Exit = true; GlobalState.Dialogue = ""; })
                .End()
                .Build();

            _state.ChangeState("Waiting");
            _state.PushState("Writing");
        }

        public override void Update()
        {
            if (KeyInput.IsRebindableKeyPressed(KeyFunctions.Accept) || KeyInput.IsRebindableKeyPressed(KeyFunctions.Cancel))
            {
                _tb.Writer.Speed = normalSpeed * 2;
            }
            else
            {
                _tb.Writer.Speed = normalSpeed;
            }

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                _state.TriggerEvent("KeyPressed");

            _tb.Update();

            _state.Update(GameTimes.DeltaTime);
        }

        public override void DrawUI()
        {
            _tb.DrawUI();
        }
    }
}
