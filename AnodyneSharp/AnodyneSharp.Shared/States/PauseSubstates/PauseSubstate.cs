using AnodyneSharp.Input;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class PauseSubstate : State
    {
        public bool Exit { get; set; }

        public virtual void GetControl()
        { }

        public virtual void HandleInput()
        {
            if (KeyInput.CanPressKey(Keys.X) || KeyInput.CanPressKey(Keys.Left))
            {
                ExitSubState();
            }
        }

        protected virtual void ExitSubState()
        {
            Exit = true;
            SoundManager.PlaySoundEffect("menu_select");
        }
    }
}
