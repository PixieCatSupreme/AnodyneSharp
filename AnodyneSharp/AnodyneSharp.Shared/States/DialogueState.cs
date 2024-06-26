﻿using AnodyneSharp.Dialogue;
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
                AddTimer(0.1f, "doBump");
            }
        }

        private TextBox _tb;

        private IState _state;

        private int normalSpeed;
        private int speedScale;

        public DialogueState(string text, bool useMenuBox = false, bool isIntro = false)
        {
            _tb = new TextBox(useMenuBox, isIntro);
            _tb.Writer.SetSpriteFont(FontManager.InitFont(Color.White), ResourceManager.GetTexture("consoleButtons"));
            _tb.Writer.Text = text;
            _tb.Writer.DrawShadow = true;

            normalSpeed = _tb.Writer.Speed;

            speedScale = isIntro ? 4 : 2;

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
                          if (_tb.Writer.AtEndOfText)
                          {
                              _state.ChangeState("Waiting");
                          }
                      })
                .End()
                .State("WaitCaret")
                    .Condition(() => GlobalState.settings.fast_text || KeyInput.IsRebindableKeyPressed(KeyFunctions.Accept) || KeyInput.IsRebindableKeyPressed(KeyFunctions.Cancel), (s) =>
                        {
                            SoundManager.PlaySoundEffect("dialogue_bloop");
                            _state.ChangeState("Writing");
                        })
                .End()
                .State<BumpState>("Bump")
                    .Enter((s) =>
                    {
                        s.linesBumped = 0;
                        _tb.BlinkyEnabled = false;
                    })
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
                             if (state.linesBumped == _tb.Writer.LinesPerBox) //Bumped all lines, move back to Waiting after writing said last line
                                 _state.ChangeState("Waiting");
                             _state.PushState("Writing");
                         }
                     })
                    .Exit((state) => _tb.BlinkyEnabled = true)
                .End()
                .State("Waiting")
                    .Condition(() => _tb.Writer.AtEndOfText && (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel)), (state) =>
                    {
                        SoundManager.PlaySoundEffect("dialogue_bloop");
                        _state.ChangeState("Done");
                    })
                    .Condition(() => !_tb.Writer.AtEndOfText && (GlobalState.settings.fast_text || KeyInput.IsRebindableKeyPressed(KeyFunctions.Accept) || KeyInput.IsRebindableKeyPressed(KeyFunctions.Cancel)), (s) =>
                    {
                        SoundManager.PlaySoundEffect("dialogue_bloop");
                        if (_tb.Writer.AtEndOfBox)
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
            if (GlobalState.settings.fast_text || KeyInput.IsRebindableKeyPressed(KeyFunctions.Accept) || KeyInput.IsRebindableKeyPressed(KeyFunctions.Cancel))
            {
                _tb.Writer.Speed = normalSpeed * speedScale;
            }
            else
            {
                _tb.Writer.Speed = normalSpeed;
            }

            _tb.Update();

            _state.Update(GameTimes.DeltaTime);
        }

        public override void DrawUI()
        {
            _tb.DrawUI();
        }
    }
}
