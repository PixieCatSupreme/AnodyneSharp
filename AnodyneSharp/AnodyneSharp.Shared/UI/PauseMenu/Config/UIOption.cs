using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public delegate void ValueChanged(string newValue, int index);
    public abstract class UIOption
    {
        public bool Enabled;
        public bool Exit;

        public ValueChanged ValueChangedEvent;

        public abstract void DrawUI();
    }
}
