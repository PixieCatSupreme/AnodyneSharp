using AnodyneSharp.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public delegate void ValueChanged(string newValue);
    public abstract class OptionSelector
    {
        public bool Enabled;
        public bool Exit;

        public ValueChanged ValueChangedEvent;

        private MenuSelector _leftArrow;
        private MenuSelector _rightArrow;

        public OptionSelector(Vector2 pos, float width)
        {
            float y = pos.Y + 2;

            _leftArrow = new MenuSelector(new Vector2(pos.X, y));
            _rightArrow = new MenuSelector(new Vector2(pos.X + width, y));

            _leftArrow.Play("enabledLeft");
            _rightArrow.Play("enabledRight");
        }

        public void Update()
        {
            _leftArrow.Update();
            _leftArrow.PostUpdate();
            _rightArrow.Update();
            _rightArrow.PostUpdate();

            if (KeyInput.CanPressKey(Keys.Left))
            {
                LeftPressed();
            }
            else if (KeyInput.CanPressKey(Keys.Right))
            {
                RightPressed();
            }
            else if (KeyInput.CanPressKey(Keys.C))
            {
                SetValue();
                Exit = true;
            }
            else if (KeyInput.CanPressKey(Keys.X))
            {
                ResetValue();
                Exit = true;
            }
        }

        public abstract void SetValue();

        public abstract void ResetValue();

        public virtual void DrawUI()
        {
            if (Enabled)
            {
                _leftArrow.Draw();
                _rightArrow.Draw();
            }
        }

        protected abstract void LeftPressed();

        protected abstract void RightPressed();
    }
}
