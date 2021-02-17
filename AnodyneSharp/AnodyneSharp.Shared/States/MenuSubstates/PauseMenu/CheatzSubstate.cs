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

namespace AnodyneSharp.States.MenuSubstates
{
    public class CheatzSubstate : Substate
    {
        private UILabel _cheatLabel;

        public CheatzSubstate()
        {
            _cheatLabel = new UILabel(new Vector2(69, 28 + GameConstants.FONT_LINE_HEIGHT * 2), true, "", forceEnglish: true);

            _selector.Position = _cheatLabel.Position - new Vector2(_selector.sprite.Width, -2);
        }

        public override void DrawUI()
        {
            _cheatLabel.Draw();
            _selector.Draw();
        }

        public override void GetControl()
        {
            base.GetControl();
        }

        public override void HandleInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                AddChar('U');
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
            {
                AddChar('R');
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {
                AddChar('D');
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
            {
                AddChar('L');
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                AddChar('1');
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
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

        private void AddChar(char c)
        {
            SoundManager.PlaySoundEffect("menu_select");
            _cheatLabel.SetText(_cheatLabel.Text + c);
        }
    }
}
