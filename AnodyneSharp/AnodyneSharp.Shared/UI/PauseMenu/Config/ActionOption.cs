using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public class ActionOption : UIOption
    {
        Action _action;

        public ActionOption(Action action)
        {
            _action = action;
        }

        public override void Update()
        {
        }

        public override void Draw()
        {
        }

        public override void GetControl()
        {
            _action();
            Exit = true;
        }

        public override void LoseControl()
        {
        }
    }
}
