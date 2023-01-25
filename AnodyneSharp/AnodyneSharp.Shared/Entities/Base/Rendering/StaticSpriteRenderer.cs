using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Resources;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Base.Rendering
{
    public class StaticSpriteRenderer : ISpriteRenderer
    {
        string textureName;
        Spritesheet sprite;
        int curFrame;

        public int Width => sprite.Width;
        public int Height => sprite.Height;

        public string CurAnimName => "";
        public bool AnimFinished => false;
        public int FrameIndex => 0;
        public int Frame => curFrame;

        public Color Color { get; set; } = Color.White;
        public ILayerType Layer { get; set; }

        public StaticSpriteRenderer(string textureName, int frameWidth, int frameHeight, int frame = 0, ILayerType layer = null) {
            this.textureName = textureName;
            sprite = new(ResourceManager.GetTexture(textureName),frameWidth,frameHeight);
            curFrame = frame;
            Layer = layer;
        }

        public void Draw(SpriteBatch batch, Vector2 position, float scale, int y_push, float rotation, float opacity, SpriteEffects flip)
        {
            Rectangle srect = sprite.GetRect(curFrame);
            srect.Height -= y_push;

            Point size = new Vector2(srect.Width * scale, srect.Height * scale).ToPoint();

            batch.Draw(sprite.Tex,
                MathUtilities.CreateRectangle(position.X + size.X / 2, position.Y + y_push + size.Y / 2, size.X, size.Y),
                srect,
                Color * opacity,
                rotation,
                new Vector2(srect.Width/2,srect.Height/2),
                flip,
                Layer.Z);
        }

        public void ReloadTexture(bool ignoreChaos = false)
        {
            sprite = new(ResourceManager.GetTexture(textureName, ignoreChaos), sprite.Width, sprite.Height);
        }

        public bool SetTexture(string textureName, int width, int height, bool ignoreChaos, bool allowFailure)
        {
            this.textureName = textureName;
            sprite = new(ResourceManager.GetTexture(textureName,ignoreChaos,allowFailure), width, height);
            return sprite.Tex != null;
        }

        public void Update()
        {
        }

        public void SetFrame(int index)
        {
            curFrame = index;
        }

        public bool PlayAnim(string name, bool force = false, int? newFramerate = null)
        {
            throw new NotImplementedException();
        }
    }
}
