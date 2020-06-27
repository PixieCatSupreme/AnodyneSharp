using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public class TextSelector : OptionSelector
    {
        public bool noConfirm;
        public bool noLoop;

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

            _textLabel = new UILabel(new Vector2(pos.X + 8, pos.Y - (GlobalState.CurrentLanguage == Dialogue.Language.ZH_CN ? GameConstants.LineOffset - 1 : 0)), true);

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
            ValueChangedEvent?.Invoke(_options[startIndex], startIndex);
            SetText();
        }

        public override void SetValue()
        {
            startIndex = index;
            ValueChangedEvent?.Invoke(_options[index], index);

            if (!noConfirm)
            {
                SoundManager.PlaySoundEffect("menu_select");
            }
        }

        public void SetValue(int i)
        {
            index = i;
            startIndex = index;
            SetText();
        }

        protected override void LeftPressed()
        {
            if (index == 0)
            {
                if (noLoop)
                {
                    return;
                }
                index = _options.Length - 1;
            }
            else
            {
                index--;
            }

            if (noConfirm)
            {
                SetValue();
            }

            SoundManager.PlaySoundEffect("menu_move");

            SetText();
        }

        protected override void RightPressed()
        {
            if (index == _options.Length - 1)
            {
                if (noLoop)
                {
                    return;
                }
                index = 0;
            }
            else
            {
                index++;
            }

            if (noConfirm)
            {
                SetValue();
            }

            SoundManager.PlaySoundEffect("menu_move");

            SetText();
        }

        private void SetText()
        {
            _textLabel.SetText(_options[index]);
        }
    }
}
