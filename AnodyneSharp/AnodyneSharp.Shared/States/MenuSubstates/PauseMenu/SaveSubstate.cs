using AnodyneSharp.Dialogue;
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
    public class SaveSubstate : Substate
    {
        private enum SaveState
        {
            SaveLabel,
            SaveTitleLable,
            SaveQuitLabel,
            QuitLabel
        }

        public bool ReturnToTitle { get; private set; }

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
            float y = 28 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = GameConstants.FONT_LINE_HEIGHT;

            _saveLabel = new UILabel(new Vector2(x, y), true, DialogueManager.GetDialogue("misc", "any", "save", 0));
            _saveTitleLable = new UILabel(new Vector2(x, _saveLabel.Position.Y + yStep * 2 - 4), true, DialogueManager.GetDialogue("misc", "any", "save", 3));
            _saveQuitLabel = new UILabel(new Vector2(x, _saveTitleLable.Position.Y + yStep * 3 - 2), true, DialogueManager.GetDialogue("misc", "any", "save", 5));
            _quitLabel = new UILabel(new Vector2(x, _saveQuitLabel.Position.Y + yStep * 2 - 2), true, DialogueManager.GetDialogue("misc", "any", "save", 6));
            _deathsLabel = new UILabel(new Vector2(x, _quitLabel.Position.Y + yStep * 4 - 2), true, DialogueManager.GetDialogue("misc", "any", "save", 7) + GlobalState.DeathCount);
        }

        public override void GetControl()
        {
            base.GetControl();
            _state = SaveState.SaveLabel;

            SetSelectorPos();
        }

        public override void HandleInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                if (_state == SaveState.SaveLabel)
                {
                    return;
                }

                _state--;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {
                if (_state >= SaveState.QuitLabel)
                {
                    return;
                }

                _state++;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                SoundManager.PlaySoundEffect("menu_select");


                bool save = true;

                switch (_state)
                {
                    case SaveState.SaveLabel:
                        _saveLabel.SetText(DialogueManager.GetDialogue("misc", "any", "save", 1));
                        break;
                    case SaveState.SaveTitleLable:
                        ReturnToTitle = true;
                        ExitSubState();
                        break;
                    case SaveState.SaveQuitLabel:
                        GlobalState.ClosingGame = true;
                        break;
                    case SaveState.QuitLabel:
                        save = false;
                        GlobalState.ClosingGame = true;
                        break;
                    default:
                        break;
                }

                if (save)
                {
                    GlobalState.SaveGame();
                }
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

            _selector.Position -= new Vector2(_selector.sprite.Width, -2 - CursorOffset);
        }

    }
}
