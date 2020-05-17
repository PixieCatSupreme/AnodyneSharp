using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.UI;
using AnodyneSharp.UI.Font;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AnodyneSharp.States
{
    public enum DialogueStateState
    {
        Writing,
        BumpingUp,
        Waiting,
        Done
    }

    public class DialogueState : State
    {
        private float bump_timer_max = 0.2f;


        private DialogueStateState state;
        private TextBox _tb;

        private bool _forcedInput;

        private float bump_timer;
        private int bumpCounter;
        private bool doBump;

        private int normalSpeed;

        public DialogueState()
        {
            Create();
        }

        public override void Create()
        {
            _tb = new TextBox();
            _tb.Writer.SetSpriteFont(FontManager.InitFont(Color.White));
            _tb.Writer.Text = GlobalState.Dialogue;
            _tb.Writer.DrawShadow = true;

            normalSpeed = _tb.Writer.Speed;

        }

        public override void Update()
        {
            if (KeyInput.IsKeyPressed(Keys.C) || KeyInput.IsKeyPressed(Keys.X))
            {
                _tb.Writer.Speed = normalSpeed * 2;
            }
            else
            {
                _tb.Writer.Speed = normalSpeed;
            }

            _tb.Update();

            switch (state)
            {
                case DialogueStateState.Writing:
                    //TODO play writing sound

                    if (_tb.Writer.NextCharacter == '^')
                    {
                        _forcedInput = true;
                        _tb.Writer.SkipCharacter();
                        _tb.PauseWriting = true;
                        state = DialogueStateState.Waiting;
                    }
                    else
                    {
                        state = (_tb.Writer.AtEndOfText || _tb.Writer.AtEndOfBox) ? DialogueStateState.Waiting : DialogueStateState.Writing;
                    }

                    break;
                case DialogueStateState.BumpingUp:

                    if (_tb.Writer.AtEndOfText)
                    {
                        state = DialogueStateState.Done;
                        break;
                    }

                    if (_tb.Writer.AtEndOfBox)
                    {
                        if (doBump)
                        {
                            _tb.Writer.RemoveFirstLine();
                            doBump = false;
                            bump_timer = 0;
                        }

                        bump_timer -= GameTimes.DeltaTime;
                        if (bump_timer > 0)
                        {
                            break;
                        }
                        bump_timer = bump_timer_max;

                        if (_tb.Writer.PushTextUp())
                        {
                            bumpCounter++;

                            _tb.Writer.ResetCursor();

                            if (bumpCounter == 3)
                            {
                                state = DialogueStateState.Writing;
                                bumpCounter = 0;
                            }
                            else
                            {
                                doBump = true;
                            }
                        }

                    }
                    break;
                case DialogueStateState.Waiting:
                    if (KeyInput.IsKeyPressed(Keys.C) || KeyInput.IsKeyPressed(Keys.X))
                    {
                        //TODO play text bloop

                        if (_tb.Writer.AtEndOfText)
                        {
                            //DONE

                            state = DialogueStateState.Done;
                        }
                        else if (_tb.Writer.AtEndOfBox)
                        {
                            //Bump up

                            state = DialogueStateState.BumpingUp;
                            bump_timer = 0;
                            doBump = true;
                        }
                        else if (_forcedInput)
                        {
                            _forcedInput = false;
                            _tb.PauseWriting = false;
                            state = DialogueStateState.Writing;
                        }
                    }
                    break;
                case DialogueStateState.Done:
                    GlobalState.Dialogue = "";
                    break;
            }
        }

        public override void DrawUI()
        {
            _tb.DrawUI();
        }
    }
}
