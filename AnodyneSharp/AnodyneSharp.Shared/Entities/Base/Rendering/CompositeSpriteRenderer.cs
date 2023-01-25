using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace AnodyneSharp.Entities.Base.Rendering
{
    public class CompositeSpriteRenderer : ISpriteRenderer
    {
        public bool AnimFinished => throw new NotImplementedException();

        public Color Color { get; set; } = Color.White;

        public string CurAnimName => throw new NotImplementedException();

        public int Frame => throw new NotImplementedException();

        public int FrameIndex => throw new NotImplementedException();

        public int Height { get; set; }

        public int Width { get; set; }
        public ISpriteRenderer[] Renderers;
        public RenderProperties[] RenderProperties;
        public ILayerType Layer { get; set; }

        public void Draw(SpriteBatch batch, Vector2 position, float scale, int y_push, float rotation, float opacity, SpriteEffects flip)
        {
            GraphicsDevice dev = batch.GraphicsDevice;
            var old_targets = dev.GetRenderTargets();

            var (spritebatch, target) = SpriteDrawer.GetRenderTarget(new(Width, Height));

            dev.SetRenderTarget(target);
            dev.Clear(Color.Transparent);

            spritebatch.Begin(SpriteSortMode.BackToFront);

            for(int i = 0; i < Renderers.Length; ++i)
            {
                var properties = RenderProperties[i];
                Renderers[i].Draw(spritebatch, properties.Position, properties.scale, properties.y_push, properties.rotation, properties.opacity, properties.flip);
            }

            spritebatch.End();

            dev.SetRenderTargets(old_targets);
            batch.Draw(target, position + target.Bounds.Size.ToVector2() / 2, null, Color*opacity, rotation, target.Bounds.Size.ToVector2() / 2, scale, flip, Layer.Z);

        }

        public bool PlayAnim(string name, bool force = false, int? newFramerate = null)
        {
            throw new NotImplementedException();
        }

        public void ReloadTexture(bool ignoreChaos = false)
        {
            foreach(var renderer in Renderers)
            {
                renderer.ReloadTexture(ignoreChaos);
            }
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
            foreach(var renderer in Renderers)
            {
                renderer.Update();
            }
        }

        public CompositeSpriteRenderer(int width, int height, params ISpriteRenderer[] renderers)
        {
            Height = height;
            Width = width;
            Renderers = renderers;
            RenderProperties = Enumerable.Range(0,renderers.Length).Select(i => new RenderProperties(Vector2.Zero)).ToArray();
        }
    }
}
