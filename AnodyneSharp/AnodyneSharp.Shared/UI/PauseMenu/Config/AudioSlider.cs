﻿using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
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
    public enum MenuStyle
    {
        PauseMenu,
        SubMenu,
        MainMenu
    }

    public class AudioSlider : OptionSelector<float>
    {
        private const float BarOffset = 8f;

        private float min;
        private float max;
        private float stepSize;
        private float current;
        private float start;

        private DrawOrder layer;

        private Spritesheet _slider;
        private Texture2D _sliderInside;
        private Texture2D _sliderBg;

        private MenuStyle _style;

        public AudioSlider(Vector2 pos, float current, float min, float max, float stepSize, MenuStyle style, DrawOrder drawOrder = DrawOrder.AUDIO_SLIDER)
            : base(pos, 68)
        {
            this.min = min;
            this.max = max;
            this.stepSize = stepSize;
            this.current = current;
            layer = drawOrder;
            start = current;

            _slider = new Spritesheet(ResourceManager.GetTexture("volume_bar", true), 60, 12);
            _sliderInside = ResourceManager.GetTexture("volume_bar_inside", true);
            _sliderBg = ResourceManager.GetTexture("volume_bar_bg", true);

            _style = style;
        }

        public override void ResetValue()
        {
            current = start;
            ValueChangedEvent?.Invoke(current, -1);
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

            ValueChangedEvent?.Invoke(current, -1);
        }

        protected override void RightPressed()
        {
            current += stepSize;

            if (current >= max)
            {
                current = max;
            }

            SoundManager.PlaySoundEffect("menu_move");

            ValueChangedEvent?.Invoke(current, -1);
        }

        public override void Draw()
        {
            base.Draw();

            float z = DrawingUtilities.GetDrawingZ(layer);
            float max_z = DrawingUtilities.GetDrawingZ(layer + 1);

            SpriteDrawer.DrawSprite(_sliderBg, new Rectangle(
                (int)(position.X + BarOffset - _slider.Width / 2),
                (int)(position.Y - _slider.Height / 2),
                _slider.Width,
                _slider.Height),
                Z: z);


            SpriteDrawer.DrawSprite(_slider.Tex, position + new Vector2(BarOffset, 0), _slider.GetRect((int)_style), Z: z + 2*(max_z - z)/3);

            if (current > min)
            {
                int width = (int)(_slider.Width * (current - min) / (max - min));

                SpriteDrawer.DrawSprite(_sliderInside, new Rectangle(
                    (int)(position.X + BarOffset - width / 2),
                    (int)(position.Y - _slider.Height / 2),
                    width,
                    _slider.Height),
                    Z: z + (max_z-z)/3);
            }
        }
    }
}
