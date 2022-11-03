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
            QuickSaveLabel,
            QuickLoadLabel,
            SaveLabel,
            SaveTitleLable,
            SaveQuitLabel,
            QuitLabel
        }

        public bool ReturnToTitle { get; private set; }

        private UILabel _quickSaveLabel;
        private UILabel _quickLoadLabel;

        private UILabel _saveLabel;
        private UILabel _saveTitleLabel;
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

            float brOffset = (GlobalState.CurrentLanguage == Language.PT_BR ? yStep + 4 : 0);

            _quickSaveLabel = new UILabel(new Vector2(x, y), true, DialogueManager.GetDialogue("misc", "any", "controls", 15));
            _quickLoadLabel = new UILabel(new Vector2(x, _quickSaveLabel.Position.Y + yStep * 2 - 4), true, DialogueManager.GetDialogue("misc", "any", "controls", 16));

            _saveLabel = new UILabel(new Vector2(x, _quickLoadLabel.Position.Y + yStep * 2 - 4 + brOffset), true, DialogueManager.GetDialogue("misc", "any", "save", 0));
            _saveTitleLabel = new UILabel(new Vector2(x, _saveLabel.Position.Y + yStep * 2 - 4), true, DialogueManager.GetDialogue("misc", "any", "save", 3));
            _saveQuitLabel = new UILabel(new Vector2(x, _saveTitleLabel.Position.Y + yStep * 3 - 2), true, DialogueManager.GetDialogue("misc", "any", "save", 5));
            _quitLabel = new UILabel(new Vector2(x, _saveQuitLabel.Position.Y + yStep * 2 - 2), true, DialogueManager.GetDialogue("misc", "any", "save", 6));
            _deathsLabel = new UILabel(new Vector2(x, _quitLabel.Position.Y + yStep * 4 - 2), true, DialogueManager.GetDialogue("misc", "any", "save", 7) + GlobalState.DeathCount);
        }

        public override void GetControl()
        {
            base.GetControl();
            _state = SaveState.QuickSaveLabel;

            SetSelectorPos();
        }

        public override void HandleInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                if (_state == SaveState.QuickSaveLabel)
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
                bool save = true;
                bool playSound = true;

                switch (_state)
                {
                    case SaveState.QuickSaveLabel:
                        GlobalState.DoQuickSave();

                        save = false;
                        playSound = false;

                        _quickSaveLabel.SetText(DialogueManager.GetDialogue("misc", "any", "save", 1));
                        break;
                    case SaveState.QuickLoadLabel:
                        save = false;
                        playSound = false;

                        GlobalState.DoQuickLoad();
                        break;
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

                if (playSound)
                {
                    SoundManager.PlaySoundEffect("menu_select");
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
            _quickSaveLabel.Draw();
            _quickLoadLabel.Draw();

            _saveLabel.Draw();
            _saveTitleLabel.Draw();
            _saveQuitLabel.Draw();
            _quitLabel.Draw();
            _deathsLabel.Draw();

            selector.Draw();
        }

        private void SetSelectorPos()
        {
            switch (_state)
            {
                case SaveState.QuickSaveLabel:
                    selector.Position = _quickSaveLabel.Position;
                    break;
                case SaveState.QuickLoadLabel:
                    selector.Position = _quickLoadLabel.Position;
                    break;
                case SaveState.SaveLabel:
                    selector.Position = _saveLabel.Position;
                    break;
                case SaveState.SaveTitleLable:
                    selector.Position = _saveTitleLabel.Position;
                    break;
                case SaveState.SaveQuitLabel:
                    selector.Position = _saveQuitLabel.Position;
                    break;
                case SaveState.QuitLabel:
                    selector.Position = _quitLabel.Position;
                    break;
            }

            selector.Position -= new Vector2(selector.sprite.Width, -2 - CursorOffset);
        }

    }
}
