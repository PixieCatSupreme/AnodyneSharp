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
        private DialogueStateState state;
        private TextBox _tb;

        private bool _forcedInput;
        private bool _removedLine;

        public DialogueState()
        {
            Create();
        }

        public override void Create()
        {
            _tb = new TextBox();
            _tb.Writer.SetSpriteFont(FontManager.InitFont(Color.White));
            _tb.Writer.Text = GlobalState.cur_dialogue;
        }

        public override void Update()
        {
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
                    if (!_removedLine)
                    {
                        _tb.Writer.RemoveFirstLine();
                        _removedLine = true;
                    }

                    state = _tb.Writer.PushTextUp() ? DialogueStateState.Writing : DialogueStateState.BumpingUp;


                    break;
                case DialogueStateState.Waiting:
                    if (KeyInput.CanPressKey(Keys.C))
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
                            _removedLine = false;
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
                    GlobalState.cur_dialogue = "";
                    break;
            }
        }

        public override void DrawUI()
        {
            _tb.DrawUI();
        }
    }
}
