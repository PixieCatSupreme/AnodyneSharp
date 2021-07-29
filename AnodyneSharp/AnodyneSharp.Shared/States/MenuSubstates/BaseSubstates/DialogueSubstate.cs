using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.MenuSubstates
{
    public class DialogueSubstate : Substate
    {
        protected bool InDialogueMode => _subsubstate != null;
        private DialogueState _subsubstate;

        public override void Update()
        {
            base.Update();

            if (InDialogueMode)
            {
                _subsubstate.Update();

                if (_subsubstate.Exit)
                {
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

            _subsubstate = new DialogueState(true);
        }
    }
}
