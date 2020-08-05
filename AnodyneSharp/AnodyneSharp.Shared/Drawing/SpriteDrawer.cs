using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Drawing
{
    public static class SpriteDrawer
    {
        public static Color BackColor;
        public static SamplerState SamplerState;
        public static Color FullScreenFade = Color.White;

        private static GraphicsDevice _graphicsDevice;
        private static SpriteBatch _spriteBatch;
        private static SpriteBatch _guiSpriteBatch;

        private static RenderTarget2D _game;
        private static RenderTarget2D _render;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            BackColor = Color.Black;
            SamplerState = SamplerState.PointWrap;

            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _guiSpriteBatch = new SpriteBatch(_graphicsDevice);

            _game = new RenderTarget2D(_graphicsDevice, 160, 160);
            _render = new RenderTarget2D(_graphicsDevice, 160, 180);
        }

        public static void DrawBackground(Background background)
        {
            _graphicsDevice.SetRenderTarget(_game);

            _graphicsDevice.Clear(BackColor);

            if (background == null)
            {
                return;
            }

            Texture2D texture = background.Texture;

            _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.AlphaBlend, samplerState: SamplerState);
            _spriteBatch.Draw(texture, background.Position, Color.White);
            _spriteBatch.End();

        }

        public static void BeginDraw(Camera camera, Effect effect = null)
        {
            _graphicsDevice.SetRenderTarget(_game);
            _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.AlphaBlend, samplerState: SamplerState, effect: effect, transformMatrix: camera.Transform);
        }

        public static void BeginGUIDraw()
        {
            _graphicsDevice.SetRenderTarget(_render);
            _graphicsDevice.Clear(BackColor);

            _guiSpriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.AlphaBlend, samplerState: SamplerState);
            _guiSpriteBatch.Draw(_game, new Rectangle(0, GameConstants.HEADER_HEIGHT, GameConstants.SCREEN_WIDTH_IN_PIXELS, GameConstants.SCREEN_HEIGHT_IN_PIXELS), Color.White);
            _guiSpriteBatch.End();

            _guiSpriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.AlphaBlend, samplerState: SamplerState);
        }



        public static void DrawSprite(Texture2D texture, Rectangle rect, Rectangle? sRect = null, Color? color = null, float rotation = 0, SpriteEffects flip = SpriteEffects.None, float Z = 0)
        {
            Rectangle r = new Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width, rect.Height);

            _spriteBatch.Draw(texture, r,
                sRect, color ?? Color.White, rotation,
                new Vector2((sRect ?? texture.Bounds).Width / 2, (sRect ?? texture.Bounds).Height / 2), flip, Z);


        }

        public static void DrawGuiSprite(Texture2D texture, Vector2 pos, Rectangle? sRect = null, Color? color = null, float rotation = 0, float scale = 1f, float Z = 0)
        {
            _guiSpriteBatch.Draw(texture, pos,
                sRect, color ?? Color.White, rotation,
                new Vector2(0), scale, SpriteEffects.None, Z);
        }

        public static void DrawGuiSprite(Texture2D texture, Rectangle rect, Rectangle? sRect = null, Color? color = null, float rotation = 0, float Z = 0)
        {
            _guiSpriteBatch.Draw(texture, rect,
                sRect, color ?? Color.White, rotation,
                new Vector2(0), SpriteEffects.None, Z);
        }

        public static void EndDraw()
        {
            _spriteBatch.End();
        }

        public static void EndGUIDraw()
        {
            _guiSpriteBatch.End();
        }

        public static void Render(Effect effect = null)
        {
            _graphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(sortMode: SpriteSortMode.Texture, blendState: BlendState.AlphaBlend, samplerState: SamplerState, effect: effect);

            _spriteBatch.Draw(_render, new Rectangle(0, 0, _graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight), FullScreenFade);

            _spriteBatch.End();
        }
    }
}
