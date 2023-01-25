using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Base.Rendering
{
    public struct RenderProperties
    {
        public Vector2 Position;
        public float scale;
        public int y_push;
        public float rotation;
        public float opacity;
        public SpriteEffects flip;

        public RenderProperties(Vector2 position = default, float scale = 1, int y_push = 0, float rotation = 0, float opacity = 1, SpriteEffects flip = default)
        {
            Position = position;
            this.scale = scale;
            this.y_push = y_push;
            this.rotation = rotation;
            this.opacity = opacity;
            this.flip = flip;
        }
    }
}
