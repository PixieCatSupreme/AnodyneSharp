using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class FlashEffect : IFullScreenEffect
    {
        FadeEffect effect = new();
        private float time = 1f;
        private bool easing = false;
        private Action onFull = null;

        public void Flash(float time, Color c, Action onFull = null)
        {
            effect.fadeColor = c * GlobalState.settings.flash_brightness;
            this.time = time;
            if (GlobalState.settings.flash_easing == 0.0f)
            {
                effect.alpha = 1.0f;
                onFull?.Invoke();
            }
            else
            {
                effect.alpha = 0.1f;
                easing = true;
                this.onFull = onFull;
            }
        }

        public void Update()
        {
            if (easing)
            {
                effect.ChangeAlpha(1f / GlobalState.settings.flash_easing);
                if (effect.alpha >= 1f)
                {
                    easing = false;
                    onFull?.Invoke();
                }
            }
            else
            {
                effect.ChangeAlpha(-1f / time);
            }
        }

        public bool Active()
        {
            return effect.Active();
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            effect.Load(content, graphicsDevice);
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            effect.Render(batch, screen);
        }

        public void ForceAlpha(float a)
        {
            effect.alpha = a;
        }

        public void Deactivate()
        {
            effect.Deactivate();
        }
    }
}
