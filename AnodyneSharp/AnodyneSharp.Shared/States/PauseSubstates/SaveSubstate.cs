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
    public class SaveSubstate : PauseSubstate
    {
        private enum SaveState
        {
            SaveLabel,
            SaveTitleLable,
            SaveQuitLabel,
            QuitLabel
        }

        private UILabel _saveLabel;
        private UILabel _saveTitleLable;
        private UILabel _saveQuitLabel;
        private UILabel _quitLabel;

        private UILabel _deathsLabel;

        private SaveState _state;
        private SaveState _lastState;

        public SaveSubstate()
        {
            float x = 69;
            float y = 28;

            _saveLabel = new UILabel(new Vector2(x, y), true);
            _saveTitleLable= new UILabel(new Vector2(x, _saveLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 2-4), true);
            _saveQuitLabel= new UILabel(new Vector2(x, _saveTitleLable.Position.Y + GameConstants.FONT_LINE_HEIGHT * 3 - 2), true);
            _quitLabel= new UILabel(new Vector2(x, _saveQuitLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 2 - 2), true);
            _deathsLabel= new UILabel(new Vector2(x, _quitLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 4 - 2), true);

            _saveLabel.Initialize();
            _saveTitleLable.Initialize();
            _saveQuitLabel.Initialize();
            _quitLabel.Initialize();
            _deathsLabel.Initialize();


            //TODO localization
            _saveLabel.SetText("Save");
            _saveTitleLable.SetText("Save and go\nto title");
            _saveQuitLabel.SetText("Save and quit");
            _quitLabel.SetText("Quit");
            _deathsLabel.SetText("Deaths: " + "0");

        }

        public override void GetControl()
        {
            base.GetControl();
            _state = SaveState.SaveLabel;

            SetSelectorPos();
        }

        public override void HandleInput()
        {
            if (KeyInput.CanPressKey(Keys.Up))
            {
                if (_state == SaveState.SaveLabel)
                {
                    return;
                }

                _state--;
            }
            else if (KeyInput.CanPressKey(Keys.Down))
            {
                if (_state >= SaveState.QuitLabel)
                {
                    return;
                }

                _state++;
            }
            else if (KeyInput.CanPressKey(Keys.C))
            {
                SoundManager.PlaySoundEffect("menu_select");
                //TODO Saving
            }
            else
            {
                base.HandleInput();
            }
        }

        public override void Update()
        {
            if (_lastState != _state)
            {
                _lastState = _state;
                SoundManager.PlaySoundEffect("menu_move");
                SetSelectorPos();
            }

            base.Update();
        }

        public override void DrawUI()
        {
            _saveLabel.Draw();
            _saveTitleLable.Draw();
            _saveQuitLabel.Draw();
            _quitLabel.Draw();
            _deathsLabel.Draw();

            _selector.Draw();
        }

        private void SetSelectorPos()
        {
            switch (_state)
            {
                case SaveState.SaveLabel:
                    _selector.Position = _saveLabel.Position;
                    break;
                case SaveState.SaveTitleLable:
                    _selector.Position = _saveTitleLable.Position;
                    break;
                case SaveState.SaveQuitLabel:
                    _selector.Position = _saveQuitLabel.Position;
                    break;
                case SaveState.QuitLabel:
                    _selector.Position = _quitLabel.Position;
                    break;
            }

            _selector.Position -= new Vector2(_selector.frameWidth, -2);
        }

    }
}
