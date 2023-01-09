using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Base.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI
{
    public class DustIcon : UIEntity
    {
        private Broom _broom;

        public static AnimatedSpriteRenderer GetSprite() => new("DustUI", 9, 9,
            new Anim("default", new int[] { 4 },1),
            new Anim("empty", new int[] { 0, 1, 2, 3, 4 },13,false),
            new Anim("full", new int[] { 4, 3, 2, 1, 0 },13,false)
            );

        public DustIcon(Vector2 pos, Broom broom)
            : base(pos, GetSprite(), DrawOrder.DUST_ICON)
        {
            _broom = broom;
        }

        public override void Update()
        {
            base.Update();

            if (CurAnimName == "full" && (_broom.just_released_dust || _broom.dust == null))
            {
                Play("empty");
            }
            else if (CurAnimName != "full" && !_broom.just_released_dust && _broom.dust != null)
            {
                Play("full");
            }
        }
    }
}
