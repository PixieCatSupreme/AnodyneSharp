using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public abstract class UIOption
    {
        public bool Exit;

        public abstract void Update();
        public abstract void Draw();

        public abstract void GetControl();
        public abstract void LoseControl();
    }
}
