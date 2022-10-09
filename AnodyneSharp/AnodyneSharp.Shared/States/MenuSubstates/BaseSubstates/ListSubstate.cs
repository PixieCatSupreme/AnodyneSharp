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
        protected bool playSfx = true;

        protected List<(UILabel label, UIOption option)> options;
        protected int state = 0;
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
            _lastState = state;

            SetSelectorPos();
        }

        public override void Update()
        {
            if (_lastState != state)
            {
                _lastState = state;
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
                    selector.visible = true;

                    _selectedOption.LoseControl();
                    _selectedOption.Exit = false;
                    _selectedOption = null;
                    state = _lastState;
                    SetSelectorPos();
                }
            }
            else
            {
                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                {
                    if (state == 0)
                    {
                        return;
                    }

                    state--;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                {
                    if (state >= options.Count - 1)
                    {
                        return;
                    }

                    state++;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                {
                    Select();
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
                selector.Draw();
            }
        }

        protected void Select()
        {
            selector.visible = false;
            _selectedOption = options[state].option;
            _selectedOption.GetControl();

            if (playSfx)
            {
                SoundManager.PlaySoundEffect("menu_select");
            }

            SetSelectorPos();
        }

        protected abstract void SetLabels();

        private void SetSelectorPos()
        {
            selector.Position = options[state].label.Position - new Vector2(selector.sprite.Width, -2);

            if (!options[state].label.ForcedEnglish)
            {
                selector.Position.Y += CursorOffset;
            }
        }
    }
}
