using AnodyneSharp.Drawing.Effects;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace AnodyneSharp.Drawing
{
    public static class SpriteDrawer
    {
        public static Color BackColor;
        public static SamplerState SamplerState;
        public static Color FullScreenFade = Color.White;

        private static GraphicsDevice _graphicsDevice;
        private static SpriteBatch _spriteBatch;

        private static RenderTarget2D _game;
        private static RenderTarget2D _game2;
        private static RenderTarget2D _render;
        private static RenderTarget2D _render2;
        private static RenderTarget2D _depth;
        private static Effect blend; //To be able to set the depth
        private static Effect _depthrender;

        public static int MaxScale => Math.Min(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 160, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 180);

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            BackColor = Color.Black;
            SamplerState = SamplerState.PointWrap;

            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);

            _game = new RenderTarget2D(_graphicsDevice, 160, 160);
            _game2 = new RenderTarget2D(_graphicsDevice, 160, 160);
            _depth = new RenderTarget2D(_graphicsDevice, 160, 160);
            _render = new RenderTarget2D(_graphicsDevice, 160, 180);
            _render2 = new RenderTarget2D(_graphicsDevice, 160, 180);
        }

        public static void Load(ContentManager c)
        {
            _depthrender = c.Load<Effect>("effects/render_depth");
            _depthrender.Parameters["World"].SetValue(Matrix.Identity);
            _depthrender.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, 160, 160, 0, 0, -1));
            blend = c.Load<Effect>("effects/blend");
        }

        public static void BeginDraw(Camera camera)
        {
            _graphicsDevice.SetRenderTargets(_game,_depth);
            _depthrender.Parameters["View"].SetValue(camera.Transform);
            _graphicsDevice.Clear(BackColor);
            _spriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.AlphaBlend, samplerState: SamplerState, effect: _depthrender, transformMatrix: camera.Transform);
        }

        public static void BeginGUIDraw()
        {
            _graphicsDevice.SetRenderTarget(_render);
            _graphicsDevice.Clear(BackColor);

            _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.AlphaBlend, samplerState: SamplerState);
            _spriteBatch.Draw(_game, new Rectangle(0, GameConstants.HEADER_HEIGHT, GameConstants.SCREEN_WIDTH_IN_PIXELS, GameConstants.SCREEN_HEIGHT_IN_PIXELS), Color.White);
            _spriteBatch.End();
            
            _spriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, blendState: BlendState.AlphaBlend, samplerState: SamplerState);
        }

        public static void DrawSprite(Texture2D texture, Rectangle rect, Rectangle? sRect = null, Color? color = null, float rotation = 0, SpriteEffects flip = SpriteEffects.None, float Z = 0)
        {
            Rectangle r = new Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width, rect.Height);

            _spriteBatch.Draw(texture, r,
                sRect, color ?? Color.White, rotation,
                new Vector2((sRect ?? texture.Bounds).Width / 2, (sRect ?? texture.Bounds).Height / 2), flip, Z);
        }

        public static void DrawSprite(Texture2D texture, Vector2 pos, Rectangle? sRect = null, Color? color = null, float rotation = 0, float scale = 1f, float Z = 0)
        {
            _spriteBatch.Draw(texture, pos,
                sRect, color ?? Color.White, rotation,
                new Vector2(0), scale, SpriteEffects.None, Z);
        }

        public static void DrawGuiSprite(Texture2D texture, Rectangle rect, Rectangle? sRect = null, Color? color = null, float rotation = 0, SpriteEffects flip = SpriteEffects.None, float Z = 0)
        {
            Rectangle r = new Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width, rect.Height);

            _spriteBatch.Draw(texture, r,
                sRect, color ?? Color.White, rotation,
                new Vector2((sRect ?? texture.Bounds).Width / 2, (sRect ?? texture.Bounds).Height / 2), flip, Z);
        }

        public static void EndDraw()
        {
            _spriteBatch.End();
            _graphicsDevice.SetRenderTarget(null);
            blend.Parameters["DepthTex"].SetValue(_depth);
            foreach(IFullScreenEffect effect in GlobalState.gameEffects.Where(e => e.Active()))
            {
                _graphicsDevice.SetRenderTarget(_game2);
                effect.Render(_spriteBatch, _game);
                (_game, _game2) = (_game2, _game);
            }
        }

        public static void EndGUIDraw()
        {
            _spriteBatch.End();

            foreach (IFullScreenEffect effect in GlobalState.fullScreenEffects.Where(e => e.Active()))
            {
                _graphicsDevice.SetRenderTarget(_render2);
                effect.Render(_spriteBatch, _render);
                (_render, _render2) = (_render2, _render);
            }
        }

        public static void Render(Effect effect = null)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(sortMode: SpriteSortMode.Texture, blendState: BlendState.AlphaBlend, samplerState: SamplerState, effect: effect);

            switch(GlobalState.settings.resolution)
            {
                case Resolution.Windowed:
                    _spriteBatch.Draw(_render, new Rectangle(0, 0, _graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight), FullScreenFade);
                    break;
                case Resolution.Scaled:
                    _spriteBatch.Draw(_render, new Rectangle(_graphicsDevice.PresentationParameters.BackBufferWidth / 2 - 160 * GlobalState.settings.scale/2, _graphicsDevice.PresentationParameters.BackBufferHeight / 2 - 180 * GlobalState.settings.scale / 2, 160 * GlobalState.settings.scale, 180 * GlobalState.settings.scale), FullScreenFade);
                    break;
                case Resolution.Stretch:
                    float scale = _graphicsDevice.PresentationParameters.BackBufferHeight / 180.0f;
                    _spriteBatch.Draw(_render, new Rectangle(_graphicsDevice.PresentationParameters.BackBufferWidth / 2 - (int)(160 * scale)/2, 0, (int)(160*scale), _graphicsDevice.PresentationParameters.BackBufferHeight), FullScreenFade);
                    break;
            }

            _spriteBatch.End();
        }
    }
}
