using AnodyneSharp.UI;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class TitleScreenOverlay : IFullScreenEffect
    {
        public Texture2D Darkness
        {
            set
            {
                blend.Parameters["OverlayTex"].SetValue(value);
            }
        }

        public List<UIEntity> Entities = new ();
        public List<UILabel> Labels = new ();

        float alpha;
        float target_alpha;

        GraphicsDevice device;
        Effect blend;

        public void TargetAlpha(float d)
        {
            target_alpha = d;
            if (alpha == 0f)
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
            MathUtilities.MoveTo(ref alpha, target_alpha, 0.8f);
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            device = graphicsDevice;
            blend = content.Load<Effect>("effects/blend");


            blend.Parameters["HardLight"].SetValue(false);
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            batch.Begin(samplerState: SamplerState.PointClamp, effect: blend);
            batch.Draw(screen, screen.Bounds, Color.White);
            batch.End();

            //Deal with darkness-wide alpha, blend darkness-applied screen with original
            if (alpha != 1f)
            {
                batch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
                batch.Draw(screen, screen.Bounds, Color.White * (1 - alpha));
                batch.End();
            }

            batch.Begin(samplerState: SamplerState.PointClamp);

            foreach (var entity in Entities)
            {
                entity.Draw();
            }

            foreach (var label in Labels)
            {
                label.Draw();
            }

            batch.End();
        }

        public void Deactivate()
        {
            alpha = 0;

            blend.Parameters["HardLight"].SetValue(false);
        }
    }
}
