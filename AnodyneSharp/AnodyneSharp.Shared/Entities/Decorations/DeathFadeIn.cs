using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Decorations
{
    public class DeathFadeIn
    {
        public float Opacity
        {
            get
            {
                return _opacity;
            }
            private set
            {
                _opacity = value;
            }
        }

        Texture2D fadeIn;
        Color color;

        float _opacity;

        public DeathFadeIn(Color color)
        {
            fadeIn = ResourceManager.GetTexture("deathFadeIn", true);
            this.color = color;
            Opacity = 0;
        }

        public void Update()
        {
            if (Opacity < 1)
            {
                MathUtilities.MoveTo(ref _opacity, 1, 0.9f);
            }
        }

        public void Draw()
        {
            SpriteDrawer.DrawSprite(fadeIn, Vector2.Zero, new Rectangle(0,0, GameConstants.SCREEN_WIDTH_IN_PIXELS, GameConstants.SCREEN_HEIGHT_IN_PIXELS + GameConstants.HEADER_HEIGHT), Z: DrawingUtilities.GetDrawingZ( DrawOrder.DEATH_FADEIN), color: color * Opacity);
        }
    }
}
