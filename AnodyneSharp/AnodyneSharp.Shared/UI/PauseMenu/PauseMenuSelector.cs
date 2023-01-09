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
    public class PauseMenuSelector : UIEntity
    {
        public static AnimatedSpriteRenderer GetSprite() => new("menu_select_active", 61, 15,
            new Anim("flash", new int[] { 0, 1 },4),
            new Anim("inactive", new int[] { 2 },1)
            );

        public PauseMenuSelector(Vector2 pos)
            : base(pos, GetSprite(), DrawOrder.PAUSE_SELECTOR)
        {
        }
    }
}
