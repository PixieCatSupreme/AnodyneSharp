using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing
{
    public class ScrollingTex
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position;
        private Vector2 _velocity;
        private DrawOrder _layer;

        public ScrollingTex(string texture, Vector2 velocity, DrawOrder layer)
        {
            Texture = ResourceManager.GetTexture(texture);
            Position = Vector2.Zero;

            _velocity = velocity;
            _layer = layer;
        }

        public void DrawUI()
        {
            Vector2 topleft = Position;
            Rectangle pos = new Rectangle((int)topleft.X, (int)topleft.Y, Texture.Width, Texture.Height);
            SpriteDrawer.DrawSprite(Texture, pos, Z: DrawingUtilities.GetDrawingZ(_layer));
        }

        public void Draw()
        {
            Vector2 topleft = Position + SpriteDrawer.Camera.Position2D;
            Rectangle pos = new Rectangle((int)topleft.X, (int)topleft.Y, Texture.Width, Texture.Height);
            SpriteDrawer.DrawSprite(Texture,pos,Z:DrawingUtilities.GetDrawingZ(_layer,0));
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
            else if (Position.Y < -Texture.Height /2)
            {
                Position.Y = 0;
            }
        }
    }
}
