using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu
{
    public class MenuSelector : UIEntity
    {
        public static AnimatedSpriteRenderer GetSprite() => new("arrows", 7, 7,
            new Anim("disabledRight", new int[] { 2 },1),
            new Anim("disabledLeft", new int[] { 0 },1),
            new Anim("enabledLeft", new int[] { 0, 1 },6),
            new Anim("enabledRight", new int[] { 3, 2 },6)
            );

        public MenuSelector(DrawOrder layer = DrawOrder.SUBMENU_SELECTOR)
            : base(Vector2.Zero, GetSprite(), layer)
        {
        }
    }
}
