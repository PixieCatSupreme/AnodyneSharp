using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class Darkness : IFullScreenEffect
    {
        Texture2D darkness;
        float alpha;

        GraphicsDevice device;
        RenderTarget2D lights_applied;
        Effect blend;

        static readonly BlendState screen = new BlendState() {
            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.InverseSourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One
        };


        public bool Active()
        {
            return alpha > 0;
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            device = graphicsDevice;
            lights_applied = new RenderTarget2D(graphicsDevice, GameConstants.SCREEN_WIDTH_IN_PIXELS, GameConstants.SCREEN_HEIGHT_IN_PIXELS);
            blend = content.Load<Effect>("effects/blend");

            blend.Parameters["HardLight"].SetValue(false);
            blend.Parameters["blendAlpha"].SetValue(1.0f);
            blend.Parameters["OverlayTex"].SetValue(lights_applied);
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            blend.Parameters["blendAlpha"].SetValue(alpha);

            RenderTargetBinding[] resultTargets = device.GetRenderTargets();

            device.SetRenderTarget(lights_applied);
            device.Clear(Color.Transparent);

            if(darkness != null)
            {
                batch.Begin();
                batch.Draw(darkness, screen.Bounds, Color.White);
                batch.End();
            }

            //todo: draw lights

            device.SetRenderTargets(resultTargets);

            if (darkness != null)
            {
                blend.Parameters["OverlayTex"].SetValue(darkness);
                batch.Begin(samplerState: SamplerState.PointClamp, effect: blend);
                batch.Draw(screen, screen.Bounds, Color.White);
                batch.End();
            }
            else
            {
                batch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
                batch.Draw(screen, screen.Bounds, Color.White);
                batch.Draw(lights_applied, lights_applied.Bounds, Color.White*alpha);
                batch.End();
            }
        }

        public void MapChange()
        {
            alpha = 1;

            string mapName = GlobalState.CURRENT_MAP_NAME.ToLower();
            darkness = ResourceManager.GetTexture(mapName + "_overlay", allowUnknown: true);
            if (darkness != null)
            {
                blend.Parameters["HardLight"].SetValue(false);
            }
            else
            {
                darkness = ResourceManager.GetTexture(mapName + "_hardlight", allowUnknown: true);
                blend.Parameters["HardLight"].SetValue(true);
            }

            if(darkness == null)
            {
                alpha = 0;
            }
        }
    }
}
