using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public class TextSelector : OptionSelector
    {
        private string[] _options;
        private int index;
        private int startIndex;

        private UILabel _textLabel;

        public TextSelector(Vector2 pos, float width, int startIndex, params string[] options)
            : base(pos, width)
        {
            _options = options;
            this.startIndex = startIndex;
            index = startIndex;

            _textLabel = new UILabel(new Vector2(pos.X + 8, pos.Y), true);

            _textLabel.Initialize();

            SetText();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            _textLabel.Draw();
        }

        public override void ResetValue()
        {
            index = startIndex;
            ValueChangedEvent?.Invoke(_options[startIndex]);
            SetText();
        }

        public override void SetValue()
        {
            startIndex = index;
            ValueChangedEvent?.Invoke(_options[index]);
        }

        protected override void LeftPressed()
        {
            if (index == 0)
            {
                index = _options.Length - 1;
            }
            else
            {
                index--;
            }

            SetText();
        }

        protected override void RightPressed()
        {
            if (index == _options.Length - 1)
            {
                index = 0;
            }
            else
            {
                index++;
            }

            SetText();
        }

        private void SetText()
        {
            _textLabel.SetText(_options[index]);
        }
    }
}
