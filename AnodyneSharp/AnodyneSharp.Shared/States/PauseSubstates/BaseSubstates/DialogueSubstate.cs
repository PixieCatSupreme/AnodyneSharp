using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class DialogueSubstate : PauseSubstate
    {
        protected bool InDialogueMode { get; private set; }
        private DialogueState _subsubstate;

        public override void Update()
        {
            base.Update();

            if (InDialogueMode)
            {
                _subsubstate.Update();

                if (GlobalState.Dialogue == "")
                {
                    InDialogueMode = false;
                    _subsubstate = null;
                }
            }
        }

        public override void DrawUI()
        {
            base.DrawUI();

            if (InDialogueMode)
            {
                _subsubstate.DrawUI();
            }
        }

        protected void SetDialogue(string text)
        {
            GlobalState.Dialogue = text;
            GlobalState.SetDialogueMode = false;

            _subsubstate = new DialogueState(true);

            InDialogueMode = true;
        }
    }
}
