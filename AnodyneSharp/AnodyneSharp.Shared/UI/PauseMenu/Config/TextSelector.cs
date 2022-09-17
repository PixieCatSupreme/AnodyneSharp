using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public class TextSelector : OptionSelector<string>
    {
        public bool noConfirm;
        public bool noLoop;

        private string[] _options;
        private int index;
        private int startIndex;

        private UILabel _textLabel;

        public TextSelector(Vector2 pos, float width, int startIndex, bool forceEnglish, Drawing.DrawOrder layer, params string[] options)
            :base(pos,width)
        {
            _options = options;
            startIndex = Math.Min(startIndex, options.Length - 1);
            this.startIndex = startIndex;
            index = startIndex;

            _textLabel = new UILabel(new Vector2(pos.X + 8, pos.Y - (!forceEnglish && GlobalState.CurrentLanguage == Dialogue.Language.ZH_CN ? GameConstants.LineOffset - 1 : 0)), true, _options[index], layer:layer, forceEnglish: forceEnglish);
        }

        public TextSelector(Vector2 pos, float width, int startIndex, bool forceEnglish, params string[] options)
            : this(pos,width,startIndex,forceEnglish,Drawing.DrawOrder.MENUTEXT,options)
        {
        }

        public override void Draw()
        {
            base.Draw();

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
