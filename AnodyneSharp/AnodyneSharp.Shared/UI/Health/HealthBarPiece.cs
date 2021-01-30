using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Resources;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI
{
    public class HealthBarPiece : UIEntity
    {
        public const int BOX_WIDTH = 11;
        public const int BOX_HEIGHT = 6;

        public const int FULL_FRAME = 0;
        public const int EMPTY_FRAME = 1;

        public HealthBarPiece(Vector2 pos, bool full)
            :base(pos, "health_piece", BOX_WIDTH, BOX_HEIGHT, DrawOrder.UI_OBJECTS)
        {
            AddAnimation("flash", CreateAnimFrameArray(0, 2), 7, true);
            AddAnimation("full", CreateAnimFrameArray(0), 0, false);
            AddAnimation("empty", CreateAnimFrameArray(1), 0, false);

            Play(full ? "full" : "empty");
        }
    }
}
