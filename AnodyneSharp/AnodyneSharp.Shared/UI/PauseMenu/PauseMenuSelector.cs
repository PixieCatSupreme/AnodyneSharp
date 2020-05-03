using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu
{
    public class PauseMenuSelector : UIEntity
    {
        public PauseMenuSelector(Vector2 pos) 
            : base(pos,61,15, DrawOrder.PAUSE_SELECTOR)
        {
            AddAnimation("flash", CreateAnimFrameArray(0, 1), 4, true);
            AddAnimation("inactive", CreateAnimFrameArray(2), 0, false);

            SetTexture("menu_select_active");

            Play("flash");
        }
    }
}
