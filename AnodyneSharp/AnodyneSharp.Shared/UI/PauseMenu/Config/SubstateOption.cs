using AnodyneSharp.States.MenuSubstates;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public class SubstateOption<T> : UIOption where T : Substate, new()
    {
        T substate;

        public override void Draw()
        {
            if(substate != null)
            {
                substate.DrawUI();
            }
        }

        public override void GetControl()
        {
            substate = new();
            substate.GetControl();
        }

        public override void LoseControl()
        {
            substate = null;
        }

        public override void Update()
        {
            if(substate != null)
            {
                substate.HandleInput();
                substate.Update();
                if(substate.Exit)
                {
                    substate = null;
                    Exit = true;
                }
            }
        }
    }
}
