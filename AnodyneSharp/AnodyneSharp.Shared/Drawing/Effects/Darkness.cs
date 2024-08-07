﻿using AnodyneSharp.Entities.Lights;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Utilities;
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
        float vel = 0.8f;

        GraphicsDevice device;
        RenderTarget2D lights_applied;
        Effect blend;
        bool hard_light = false;

        List<Light> lights = new List<Light>();

        static readonly BlendState screenblend = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
            ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceColor,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.Zero,
            AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One
        };

        static readonly BlendState multiply = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.DestinationColor,
            ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.Zero
        };

        //Called during draw phase of entities
        public void AddLight(Light light)
        {
            if (!Active()) return;
            lights.Add(light);
        }

        public void TargetAlpha(float d, float vel = 0.8f)
        {
            this.vel = vel;
            target_alpha = d;
            if (alpha == 0f && d != 0f)
            {
                alpha = 0.1f;
            }
        }

        public float Alpha => alpha;

        public bool ReachedAlpha => alpha == target_alpha;

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
            MathUtilities.MoveTo(ref alpha, target_alpha, vel);
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            device = graphicsDevice;
            lights_applied = new RenderTarget2D(graphicsDevice, GameConstants.SCREEN_WIDTH_IN_PIXELS, GameConstants.SCREEN_HEIGHT_IN_PIXELS);
            blend = content.Load<Effect>("effects/blend");
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {

            RenderTargetBinding[] resultTargets = device.GetRenderTargets();

            device.SetRenderTarget(lights_applied);

            if (darkness != null)
            {
                device.Clear(Color.Transparent);
                batch.Begin();
                batch.Draw(darkness, screen.Bounds, Color.White);
                batch.End();
            }
            else
            {
                device.Clear(Color.Black);
            }

            if (lights.Count > 0)
            {
                batch.Begin(SpriteSortMode.Immediate, blendState: screenblend, samplerState: SamplerState.PointClamp, transformMatrix: SpriteDrawer.Camera.Transform);
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
                blend.Parameters["HardLight"].SetValue(hard_light);
                blend.Parameters["DepthCutoff"].SetValue(0f);
                blend.Parameters["Projection"].SetValue(SpriteDrawer.Projection(screen.Bounds.Size));
                batch.Begin(samplerState: SamplerState.PointClamp, effect: blend);
                batch.Draw(screen, screen.Bounds, Color.White);
                batch.End();
            }
            else
            {
                device.Clear(Color.White);

                batch.Begin(SpriteSortMode.Immediate, blendState: multiply, samplerState: SamplerState.PointClamp);
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

        public void Deactivate()
        {
            alpha = 0;
        }

        public void SetTex(string texName)
        {
            darkness = ResourceManager.GetTexture(texName, allowUnknown: true);
            hard_light = texName.EndsWith("hardlight");
        }
    }
}
