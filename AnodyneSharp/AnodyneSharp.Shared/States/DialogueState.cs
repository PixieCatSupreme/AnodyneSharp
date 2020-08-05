using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
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
        private int linesBumped;
        private bool doBump;
        private int currentLineBump;

        private int normalSpeed;

        public DialogueState(bool useMenuBox = false)
        {
            _tb = new TextBox(useMenuBox);
            _tb.Writer.SetSpriteFont(FontManager.InitFont(Color.White), ResourceManager.GetTexture("consoleButtons"));
            _tb.Writer.Text = GlobalState.Dialogue;
            _tb.Writer.DrawShadow = true;

            normalSpeed = _tb.Writer.Speed;
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

            _tb.Update();

            switch (state)
            {
                case DialogueStateState.Writing:
                    SoundManager.PlaySoundEffect("dialogue_blip");

                    if (_tb.Writer.NextCharacter == '^')
                    {
                        _forcedInput = true;
                        _tb.Writer.SkipCharacter();
                        _tb.PauseWriting = true;
                        state = DialogueStateState.Waiting;
                    }
                    else
                    {
                        bool atEnd = _tb.Writer.AtEndOfText || _tb.Writer.AtEndOfBox;
                        if (atEnd)
                        {
                            if (linesBumped != 0)
                            {
                                state = DialogueStateState.BumpingUp;
                                doBump = true;
                            }
                            else
                            {
                                state = DialogueStateState.Waiting;
                            }
                        }
                    }

                    break;
                case DialogueStateState.BumpingUp:

                    if (_tb.Writer.AtEndOfText)
                    {
                        state = DialogueStateState.Waiting;
                        break;
                    }


                    bump_timer -= GameTimes.DeltaTime;
                    if (bump_timer > 0)
                    {
                        break;
                    }
                    bump_timer = bump_timer_max;


                    if (doBump)
                    {
                        _tb.Writer.RemoveFirstLine();
                        doBump = false;
                    }

                    if (currentLineBump < 2)
                    {
                        _tb.Writer.PushTextUp();
                        currentLineBump++;
                    }
                    else if (linesBumped < 3)
                    {
                        if (linesBumped < 2)
                        {
                            currentLineBump = 0;
                        }
                        linesBumped++;
                        if (linesBumped == 3)
                        {
                            linesBumped = 0;
                        }

                        _tb.Writer.ResetCursor();
                        state = DialogueStateState.Writing;
                    }
                    break;
                case DialogueStateState.Waiting:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        SoundManager.PlaySoundEffect("dialogue_bloop");

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
                            currentLineBump = 0;
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
                    Exit = true;
                    break;
            }
        }

        public override void DrawUI()
        {
            _tb.DrawUI();
        }
    }
}
