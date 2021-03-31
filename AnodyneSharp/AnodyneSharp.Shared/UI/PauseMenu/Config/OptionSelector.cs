using AnodyneSharp.Input;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.UI.PauseMenu.Config
{
    public abstract class OptionSelector<T> : UIOption
    {
        public delegate void ValueChanged(T newValue, int index);
        
        public ValueChanged ValueChangedEvent;

        private bool Enabled;

        protected Vector2 position;

        private MenuSelector _leftArrow;
        private MenuSelector _rightArrow;

        public OptionSelector(Vector2 pos, float width)
        {
            position = pos;
            float y = pos.Y + 2;

            _leftArrow = new MenuSelector();
            _rightArrow = new MenuSelector();

            _leftArrow.Position = new Vector2(pos.X, y);
            _rightArrow.Position = new Vector2(pos.X + width, y);

            _leftArrow.Play("enabledLeft");
            _rightArrow.Play("enabledRight");
        }

        public override void GetControl()
        {
            Enabled = true;
        }

        public override void LoseControl()
        {
            Enabled = false;
        }

        public override void Update()
        {
            _leftArrow.Update();
            _leftArrow.PostUpdate();
            _rightArrow.Update();
            _rightArrow.PostUpdate();

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
            {
                LeftPressed();
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
            {
                RightPressed();
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                SetValue();
                Exit = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
            {
                ResetValue();
                Exit = true;
            }
        }

        public abstract void SetValue();

        public abstract void ResetValue();

        public override void Draw()
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
