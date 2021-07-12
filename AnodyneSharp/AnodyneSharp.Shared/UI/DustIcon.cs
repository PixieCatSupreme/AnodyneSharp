using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI
{
    public class DustIcon : UIEntity
    {
        private Broom _broom;

        public DustIcon(Vector2 pos, Broom broom)
            : base(pos, "DustUI", 9, 9, DrawOrder.DUST_ICON)
        {
            AddAnimation("empty", CreateAnimFrameArray(0, 1, 2, 3, 4), 13, false);
            AddAnimation("full", CreateAnimFrameArray(4, 3, 2, 1, 0), 13, false);
            AddAnimation("default", CreateAnimFrameArray(4), 0, false);

            Play("default");

            _broom = broom;
        }

        public override void Update()
        {
            base.Update();

            if (_curAnim.name == "full" && (_broom.just_released_dust || _broom.dust == null))
            {
                Play("empty");
            }
            else if (_curAnim.name != "full" && !_broom.just_released_dust && _broom.dust != null)
            {
                Play("full");
            }
        }
    }
}
