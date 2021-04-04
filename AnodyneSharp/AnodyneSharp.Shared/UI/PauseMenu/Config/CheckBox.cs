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
        private bool _useMainMenuFrame;
        private DrawOrder _layer;

        public CheckBox(Vector2 position, bool current, bool useMainMenuFrame, DrawOrder layer = DrawOrder.AUDIO_SLIDER)
        {
            _box = new Spritesheet(ResourceManager.GetTexture("checkbox", true), 11, 11);

            _pos = position;

            _turnedOn = current;

            _layer = layer;

            _useMainMenuFrame = useMainMenuFrame;
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
            SpriteDrawer.DrawGuiSprite(_box.Tex, _pos,
                _box.GetRect((_turnedOn ? 1 : 0) + (_useMainMenuFrame ? 2 : 0)), 
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
