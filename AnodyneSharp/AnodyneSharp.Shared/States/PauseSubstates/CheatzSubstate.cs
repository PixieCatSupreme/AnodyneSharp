using AnodyneSharp.Cheatz;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class CheatzSubstate : PauseSubstate
    {
        private UILabel _cheatLabel;
        private MenuSelector _selector;

        public CheatzSubstate()
        {
            _cheatLabel = new UILabel(new Vector2(69, 28 + GameConstants.FONT_LINE_HEIGHT * 2), true);
            _cheatLabel.Initialize();
            _cheatLabel.SetText("");

            _selector = new MenuSelector()
            {
                visible = false
            };

            _selector.Position = _cheatLabel.Position - new Vector2(_selector.frameWidth, -2);
        }

        public override void DrawUI()
        {
            _cheatLabel.Draw();
            _selector.Draw();
        }

        public override void GetControl()
        {
            _selector.visible = true;
            _selector.Play("enabledRight");
        }

        public override void Update()
        {
            _selector.Update();
            _selector.PostUpdate();
        }

        public override void HandleInput()
        {
            if (KeyInput.CanPressKey(Keys.Up))
            {
                AddChar('U');
            }
            else if (KeyInput.CanPressKey(Keys.Right))
            {
                AddChar('R');
            }
            else if (KeyInput.CanPressKey(Keys.Down))
            {
                AddChar('D');
            }
            else if (KeyInput.CanPressKey(Keys.Left))
            {
                AddChar('L');
            }
            else if (KeyInput.CanPressKey(Keys.C))
            {
                AddChar('1');
            }
            else if (KeyInput.CanPressKey(Keys.X))
            {
                AddChar('2');
            }

            if (_cheatLabel.Text.Length == 10)
            {
                CheatzManager.DoCheat(_cheatLabel.Text);
                _cheatLabel.SetText("");
                ExitSubState();
            }
        }

        protected override void ExitSubState()
        {
            Exit = true;
            _selector.visible = false;
        }

        private void AddChar(char c)
        {
            SoundManager.PlaySoundEffect("menu_select");
            _cheatLabel.SetText(_cheatLabel.Text + c);
        }
    }
}
