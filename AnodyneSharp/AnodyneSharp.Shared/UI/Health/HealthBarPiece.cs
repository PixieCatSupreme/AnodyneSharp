using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Base.Rendering;
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

        public static AnimatedSpriteRenderer GetSprite() => new("health_piece", BOX_WIDTH, BOX_HEIGHT,
            new Anim("full", new int[] { 0 }, 1),
            new Anim("empty", new int[] { 1 },1),
            new Anim("flash",new int[] { 0, 2 },7)
            );

        public HealthBarPiece(Vector2 pos, bool full)
            :base(pos, GetSprite(), DrawOrder.UI_OBJECTS)
        {
            Play(full ? "full" : "empty");
        }
    }
}
