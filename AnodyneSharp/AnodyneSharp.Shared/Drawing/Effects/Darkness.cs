using AnodyneSharp.Entities.Lights;
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
        float target_alpha;

        GraphicsDevice device;
        RenderTarget2D lights_applied;
        Effect blend;

        Camera camera;
        List<Light> lights = new List<Light>();

        static readonly BlendState screenblend = new BlendState() {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.InverseSourceColor,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One
        };

        static readonly BlendState multiply = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.DestinationColor,
            ColorDestinationBlend = Blend.Zero
        };

        //Called during draw phase of entities
        public void AddLight(Light light)
        {
            if (!Active()) return;
            lights.Add(light);
        }

        public void TargetAlpha(float d)
        {
            target_alpha = d;
            if(alpha == 0f)
            {
                alpha = 0.1f;
            }
        }

        public void ForceAlpha(float a)
        {
            target_alpha = alpha = a;
        }

        public bool Active()
        {
            return alpha > 0;
        }

        public void Update()
        {
            if (alpha != target_alpha)
            {
                if (alpha > target_alpha)
                {
                    alpha = Math.Max(alpha - 0.8f * GameTimes.DeltaTime, target_alpha);
                }
                else
                {
                    alpha = Math.Min(alpha + 0.8f * GameTimes.DeltaTime, target_alpha);
                }
            }
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            device = graphicsDevice;
            lights_applied = new RenderTarget2D(graphicsDevice, GameConstants.SCREEN_WIDTH_IN_PIXELS, GameConstants.SCREEN_HEIGHT_IN_PIXELS);
            blend = content.Load<Effect>("effects/blend");

            blend.Parameters["HardLight"].SetValue(false);
            blend.Parameters["OverlayTex"].SetValue(lights_applied);
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {

            RenderTargetBinding[] resultTargets = device.GetRenderTargets();

            device.SetRenderTarget(lights_applied);
            device.Clear(Color.Transparent);

            if(darkness != null)
            {
                batch.Begin();
                batch.Draw(darkness, screen.Bounds, Color.White);
                batch.End();
            }
            else
            {
                device.Clear(Color.Black);
            }

            if(lights.Count > 0)
            {
                batch.Begin(SpriteSortMode.Immediate, blendState: screenblend, samplerState: SamplerState.PointClamp, transformMatrix:camera.Transform);
                foreach (Light light in lights)
                {
                    light.DrawLight();
                }
                batch.End();
                lights.Clear();
            }

            device.SetRenderTargets(resultTargets);

            if (darkness != null)
            {
                blend.Parameters["OverlayTex"].SetValue(lights_applied);
                batch.Begin(samplerState: SamplerState.PointClamp, effect: blend);
                batch.Draw(screen, screen.Bounds, Color.White);
                batch.End();
            }
            else
            {
                device.Clear(Color.White);

                batch.Begin(SpriteSortMode.Immediate, blendState:multiply, samplerState: SamplerState.PointClamp);
                batch.Draw(screen, screen.Bounds, Color.White);
                batch.Draw(lights_applied, lights_applied.Bounds, Color.White);
                batch.End();
            }

            //Deal with darkness-wide alpha, blend darkness-applied screen with original
            if (alpha != 1f)
            {
                batch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
                batch.Draw(screen, screen.Bounds, Color.White * (1 - alpha));
                batch.End();
            }

        }

        internal void SetCamera(Camera camera)
        {
            this.camera = camera;
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

            target_alpha = alpha;
        }
    }
}
