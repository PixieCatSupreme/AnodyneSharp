using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public class CheckBox : UIOption
    {
        public delegate void ValueChanged(bool newValue);

        public ValueChanged ValueChangedEvent;

        private Spritesheet _box;

        private Vector2 _pos;
        private bool _turnedOn;
        private MenuStyle _style;

        private DrawOrder _layer;

        public CheckBox(Vector2 position, bool current, MenuStyle style, DrawOrder layer = DrawOrder.AUDIO_SLIDER)
        {
            _box = new Spritesheet(ResourceManager.GetTexture("checkbox", true), 11, 11);

            _pos = position;

            _turnedOn = current;

            _layer = layer;

            _style = style;
        }

        public void Toggle()
        {
            _turnedOn = !_turnedOn;
            ValueChangedEvent?.Invoke(_turnedOn);
        }

        public override void Update()
        {
        }

        public override void Draw()
        {
            SpriteDrawer.DrawSprite(_box.Tex, _pos,
                _box.GetRect((_turnedOn ? 1 : 0) + ((int)_style * 2)), 
                Z: DrawingUtilities.GetDrawingZ(_layer));

        }

        public override void GetControl()
        {
            Toggle();
            Exit = true;
        }

        public override void LoseControl()
        {
        }
    }
}
