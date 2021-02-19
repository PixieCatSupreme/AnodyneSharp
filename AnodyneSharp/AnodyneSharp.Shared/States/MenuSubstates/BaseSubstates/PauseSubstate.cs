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
        public new bool Exit { get; set; }

        protected MenuSelector _selector;

        public Substate()
        {
            _selector = new MenuSelector()
            {
                visible = false
            };
        }

        public override void Update()
        {
            base.Update();

            _selector.Update();
            _selector.PostUpdate();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            _selector.Draw();
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
            _selector.visible = true;
            _selector.Play("enabledRight");
        }

        protected virtual void OnExit() { }

        protected void ExitSubState()
        {
            Exit = true;
            _selector.visible = false;
            SoundManager.PlaySoundEffect("menu_select");
            OnExit();
        }
    }
}
