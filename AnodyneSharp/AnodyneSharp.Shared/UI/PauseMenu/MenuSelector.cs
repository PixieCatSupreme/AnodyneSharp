using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu
{
    public class MenuSelector : UIEntity
    {
        public MenuSelector(Vector2 pos)
            : base(pos, 7, 7, DrawOrder.SUBMENU_SELECTOR)
        {
            AddAnimation("disabledLeft", CreateAnimFrameArray(0), 0, false);
            AddAnimation("disabledRight", CreateAnimFrameArray(2), 0, false);
            AddAnimation("enabledLeft", CreateAnimFrameArray(0, 1), 6, true);
            AddAnimation("enabledRight", CreateAnimFrameArray(3, 2), 6, true);

            Texture = ResourceManager.GetTexture("arrows");

            Play("disabledRight");
        }
    }
}
