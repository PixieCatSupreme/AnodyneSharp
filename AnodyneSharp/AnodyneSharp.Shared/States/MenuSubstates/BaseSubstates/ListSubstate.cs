using AnodyneSharp.Input;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu.Config;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.MenuSubstates
{
    public abstract class ListSubstate : Substate
    {
        protected List<(UILabel label, UIOption option)> options;
        private int _state = 0;
        private int _lastState = 0;

        private UIOption _selectedOption;

        private bool _leftExits;

        public ListSubstate(bool leftExits = false)
        {
            _leftExits = leftExits;
        }

        public override void GetControl()
        {
            base.GetControl();
            _lastState = _state;

            SetSelectorPos();
        }

        public override void Update()
        {
            if (_lastState != _state)
            {
                _lastState = _state;
                SetSelectorPos();
                SoundManager.PlaySoundEffect("menu_move");
            }

            base.Update();
        }

        public override void HandleInput()
        {
            if (_selectedOption != null)
            {
                _selectedOption.Update();

                if (_selectedOption.Exit)
                {
                    _selector.visible = true;

                    _selectedOption.LoseControl();
                    _selectedOption.Exit = false;
                    _selectedOption = null;
                    _state = _lastState;
                    SetSelectorPos();
                }
            }
            else
            {
                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                {
                    if (_state == 0)
                    {
                        return;
                    }

                    _state--;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                {
                    if (_state >= options.Count - 1)
                    {
                        return;
                    }

                    _state++;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                {
                    _selector.visible = false;
                    _selectedOption = options[_state].option;
                    _selectedOption.GetControl();

                    SoundManager.PlaySoundEffect("menu_select");

                    SetSelectorPos();
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel) || (_leftExits && KeyInput.JustPressedRebindableKey(KeyFunctions.Left)))
                {
                    ExitSubState();
                }
            }
        }

        public override void DrawUI()
        {
            foreach (var (label, option) in options)
            {
                label.Draw();
                option.Draw();
            }

            if (_selectedOption == null)
            {
                _selector.Draw();
            }
        }

        protected abstract void SetLabels();

        private void SetSelectorPos()
        {
            _selector.Position = options[_state].label.Position - new Vector2(_selector.sprite.Width, -2);

            if (!options[_state].label.ForcedEnglish)
            {
                _selector.Position.Y += CursorOffset;
            }
        }
    }
}
