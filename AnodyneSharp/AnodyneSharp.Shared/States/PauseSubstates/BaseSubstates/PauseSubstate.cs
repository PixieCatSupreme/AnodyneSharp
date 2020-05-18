using AnodyneSharp.Input;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class PauseSubstate : State
    {
        public bool Exit { get; set; }

        protected MenuSelector _selector;

        public PauseSubstate()
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
            if (KeyInput.CanPressKey(Keys.X) || KeyInput.CanPressKey(Keys.Left))
            {
                ExitSubState();
            }
        }

        public virtual void GetControl()
        {
            _selector.visible = true;
            _selector.Play("enabledRight");
        }

        protected void ExitSubState()
        {
            Exit = true;
            _selector.visible = false;
            SoundManager.PlaySoundEffect("menu_select");
            _selector.Play("disabledRight");
        }
    }
}
