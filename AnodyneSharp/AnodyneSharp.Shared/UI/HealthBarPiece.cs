using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI
{
    public class HealthBarPiece : Entity
    {
        public const int BOX_WIDTH = 11;
        public const int BOX_HEIGHT = 6;
        public const int BOX_SPACING = -3;

        public const int FULL_FRAME = 0;
        public const int EMPTY_FRAME = 1;

        public static Texture2D Health_Piece_Sprite;

        public static void SetSprites(ContentManager content)
        {
            Health_Piece_Sprite = TextureUtilities.LoadTexture("sprites/inventory", "health_piece", content);
        }

        public HealthBarPiece(Vector2 pos)
            :base(pos, BOX_WIDTH, BOX_HEIGHT)
        {
            AddAnimation("flash", CreateAnimFrameArray(0, 2), 7, true);
            AddAnimation("full", CreateAnimFrameArray(0), 0, false);
            AddAnimation("empty", CreateAnimFrameArray(1), 0, false);

            Texture = Health_Piece_Sprite;

            Play("full");
        }


        public override void Draw()
        {
            SpriteDrawer.DrawGuiSprite(Texture, Position - offset, spriteRect, Z: 0.2f);
        }

    }
}
