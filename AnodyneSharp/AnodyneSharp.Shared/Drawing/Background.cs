using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing
{
    public class Background
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position;
        private Vector2 _velocity;

        public Background(string texture, Vector2 velocity)
        {
            Texture = ResourceManager.GetTexture(texture);
            Position = Vector2.Zero;

            _velocity = velocity;
        }

        public void Update()
        {
            Position += _velocity * GameTimes.DeltaTime;

            if (Position.X < -Texture.Width /2)
            {
                Position.X = 0;
            }

            if (Position.Y > 0)
            {
                Position.Y = -Texture.Height / 2;
            }
        }
    }
}
