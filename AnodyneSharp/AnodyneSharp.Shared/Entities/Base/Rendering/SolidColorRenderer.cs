using AnodyneSharp.Drawing;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Base.Rendering
{
    public class SolidColorRenderer : ISpriteRenderer
    {
        public bool AnimFinished => throw new NotImplementedException();

        public Color Color { get; set; } = Color.White;

        public string CurAnimName => throw new NotImplementedException();

        public int Frame => 0;

        public int FrameIndex => 0;

        public int Height { get; set; }

        public int Width { get; set; }

        public ILayerType Layer { get; set; }

        public SolidColorRenderer(Color color, int height, int width)
        {
            Color = color;
            Height = height;
            Width = width;
        }

        public void Draw(SpriteBatch batch, Vector2 position, float scale, int y_push, float rotation, float opacity, SpriteEffects flip)
        {
            //TODO? make it use the batch directly, not necessary for current project
            SpriteDrawer.DrawSprite(SpriteDrawer.SolidTex,
                new Rectangle((int)(position.X + Width*(1-scale)/2), (int)(position.Y + y_push + (Height-y_push)*(1-scale)/2), (int)(Width * scale), (int)((Height - y_push) * scale)),
                null,
                Color*opacity,
                rotation,
                flip,
                Layer.Z);
        }

        public bool PlayAnim(string name, bool force = false, int? newFramerate = null)
        {
            throw new NotImplementedException();
        }

        public void ReloadTexture(bool ignoreChaos = false)
        {
        }

        public void SetFrame(int index)
        {
            throw new NotImplementedException();
        }

        public bool SetTexture(string textureName, int width, int height, bool ignoreChaos, bool allowFailure)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
        }
    }
}
