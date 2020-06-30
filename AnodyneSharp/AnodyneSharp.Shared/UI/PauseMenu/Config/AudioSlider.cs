using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public class AudioSlider : OptionSelector
    {
        private const float BarOffset = 8f;

        private float min;
        private float max;
        private float stepSize;
        private float current;
        private float start;

        private Texture2D _slider;
        private Texture2D _sliderInside;
        private Texture2D _sliderBg;

        public AudioSlider(Vector2 pos, float current, float min, float max, float stepSize)
            : base(pos, 68)
        {
            this.min = min;
            this.max = max;
            this.stepSize = stepSize;
            this.current = current;
            start = current;

            _slider = ResourceManager.GetTexture("volume_bar", true);
            _sliderInside = ResourceManager.GetTexture("volume_bar_inside", true);
            _sliderBg = ResourceManager.GetTexture("volume_bar_bg", true);
        }

        public override void ResetValue()
        {
            current = start;
            ValueChangedEvent?.Invoke(current.ToString(), -1);
            SoundManager.PlaySoundEffect("menu_select");
        }

        public override void SetValue()
        {
            start = current;
            SoundManager.PlaySoundEffect("menu_select");
        }

        protected override void LeftPressed()
        {
            current -= stepSize;

            if (current <= min)
            {
                current = min;
            }

            SoundManager.PlaySoundEffect("menu_move");

            ValueChangedEvent?.Invoke(current.ToString(), -1);
        }

        protected override void RightPressed()
        {
            current += stepSize;

            if (current >= max)
            {
                current = max;
            }

            SoundManager.PlaySoundEffect("menu_move");

            ValueChangedEvent?.Invoke(current.ToString(), -1);
        }

        public override void DrawUI()
        {
            base.DrawUI();

            SpriteDrawer.DrawGuiSprite(_sliderBg, new Rectangle(
                (int)((position.X + BarOffset) ),
                (int)(position.Y ),
                (int)(_slider.Width ),
                (int)(_slider.Height )),
                Z: DrawingUtilities.GetDrawingZ(DrawOrder.AUDIO_SLIDER_BG));


            SpriteDrawer.DrawGuiSprite(_slider, position + new Vector2(BarOffset, 0), Z: DrawingUtilities.GetDrawingZ(DrawOrder.AUDIO_SLIDER));

            if (current > min)
            {
                SpriteDrawer.DrawGuiSprite(_sliderInside, new Rectangle(
                    (int)((position.X + BarOffset) ),
                    (int)(position.Y ),
                    (int)(_slider.Width  * (current - min) / (max - min)),
                    (int)(_slider.Height )),
                    Z: DrawingUtilities.GetDrawingZ(DrawOrder.AUDIO_SLIDER_BAR));
            }
        }
    }
}
