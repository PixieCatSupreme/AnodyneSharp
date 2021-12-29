using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.MenuSubstates
{
    public class Substate : State
    {
        protected int CursorOffset
        {
            get
            {
                if (GlobalState.CurrentLanguage == Dialogue.Language.ZH_CN )
                {
                    return GameConstants.LineOffset-1;
                }
                else
                {
                    return 0;
                }
            }
        }
        //public new bool Exit { get; set; }

        protected MenuSelector selector;

        public Substate()
        {
            selector = new MenuSelector()
            {
                visible = false
            };
        }

        public override void Update()
        {
            base.Update();

            selector.Update();
            selector.PostUpdate();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            selector.Draw();
        }

        public virtual void HandleInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel) || KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
            {
                ExitSubState();
            }
        }

        public virtual void GetControl()
        {
            selector.visible = true;
            selector.Play("enabledRight");
        }

        protected virtual void OnExit() { }

        protected void ExitSubState()
        {
            Exit = true;
            selector.visible = false;
            SoundManager.PlaySoundEffect("menu_select");
            OnExit();
        }
    }
}
