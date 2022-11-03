using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnodyneSharp.States.MenuSubstates.MainMenu
{
    public class FileSubstate : Substate
    {
        private enum State
        {
            Game,
            NewGame,
            YesLabel,
            NoLabel,
            Copy,
            Save1Label,
            Save2Label,
            Save3Label,
            CancelLabel
        }

        public bool LoadedSave { get; private set; }
        public bool NewSave { get; private set; }

        public bool RefreshSaves { get; private set; }

        private State _state;
        private State _lastState;

        private int _saveID;
        private bool SaveExists => Save != null;

        public GlobalState.Save Save { get; private set; }

        private int confirmState;

        private UILabel _gameLabel;
        private UILabel _newGameLabel;
        private UILabel _copyLabel;

        private UILabel _confirmLabel;
        private UILabel _yesLabel;
        private UILabel _noLabel;

        private UILabel _copyToLabel;

        private UILabel _save1Label;
        private UILabel _save2Label;
        private UILabel _save3Label;
        private UILabel _cancelLabel;

        private HealthBar healthBar;

        private UILabel _timeLabel;
        private UILabel _deathLabel;
        private UILabel _cardLabel;

        private bool _showHealth;
        private bool _copyMode;

        private int copyFileId;

        public FileSubstate(int saveID)
        {
            _saveID = saveID;

            Save = GlobalState.Save.GetSave($"{GameConstants.SavePath}Saves/Save_{saveID + 1}.dat");

            confirmState = 0;

            SetLabels();

            _showHealth = SaveExists;
        }

        public override void DrawUI()
        {
            base.DrawUI();

            _gameLabel.Draw();
            _newGameLabel.Draw();
            _copyLabel.Draw();

            _confirmLabel.Draw();
            _yesLabel.Draw();
            _noLabel.Draw();

            _copyToLabel.Draw();
            _save1Label.Draw();
            _save2Label.Draw();
            _save3Label.Draw();
            _cancelLabel.Draw();

            _timeLabel.Draw();
            _deathLabel.Draw();
            _cardLabel.Draw();


            //Healthbar has no visible
            if (_showHealth)
            {
                healthBar.Draw();
            }
        }

        public override void Update()
        {
            if (_lastState != _state)
            {
                _lastState = _state;
                SetSelectorPos();
            }

            base.Update();
        }

        public override void GetControl()
        {
            base.GetControl();

            RefreshSaves = false;
            SetSelectorPos();
        }

        public override void HandleInput()
        {
            bool moved = false;
            bool selected = false;

            switch (_state)
            {
                case State.Game:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        GlobalState.CurrentSaveGame = _saveID;

                        selected = true;

                        if (SaveExists)
                        {
                            GlobalState.ResetValues();
                            GlobalState.LoadSave(Save);
                        }
                        else
                        {
                            NewSave = true;

                            GlobalState.ResetValues();
                        }

                        LoadedSave = true;

                        Exit = true;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;

                        if (SaveExists)
                        {
                            _state = State.NewGame;
                        }
                    }
                    else
                    {
                        base.HandleInput();
                    }
                    break;
                case State.NewGame:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        _state = State.NoLabel;
                        selected = true;

                        SetConfirmation();

                        _confirmLabel.IsVisible = true;
                        _yesLabel.IsVisible = true;
                        _noLabel.IsVisible = true;

                        _gameLabel.IsVisible = false;
                        _newGameLabel.IsVisible = false;
                        _copyLabel.IsVisible = false;

                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;

                        _state = State.Game;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;

                        _state = State.Copy;
                    }
                    else
                    {
                        base.HandleInput();
                    }
                    break;
                case State.YesLabel:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        selected = true;

                        if (confirmState == 2)
                        {
                            if (_copyMode)
                            {
                                DoCopy();
                            }
                            else
                            {
                                DeletedSave();
                            }
                        }
                        else
                        {
                            _state = State.NoLabel;

                            confirmState++;

                            SetConfirmation();
                        }
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        selected = true;

                        if (_copyMode)
                        {
                            EnterCopy();
                        }
                        else
                        {
                            ExitConfirmation();
                        }
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;

                        _state = State.NoLabel;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;
                    }
                    break;
                case State.NoLabel:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        selected = true;

                        if (_copyMode)
                        {
                            EnterCopy();
                        }
                        else
                        {
                            ExitConfirmation();
                        }
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;

                        _state = State.YesLabel;
                    }
                    break;
                case State.Copy:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        selected = true;

                        EnterCopy();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;

                        _state = State.NewGame;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;
                    }
                    else
                    {
                        base.HandleInput();
                    }
                    break;
                case State.Save1Label:

                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        copyFileId = 0;
                        CopyCheck();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        selected = true;

                        ExitCopy();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;

                        _state = _saveID == 1 ? State.Save3Label : State.Save2Label;
                    }
                    break;
                case State.Save2Label:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        copyFileId = 1;
                        CopyCheck();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        selected = true;

                        ExitCopy();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;

                        if (_saveID != 0)
                        {
                            _state = State.Save1Label;
                        }
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;

                        if (_saveID != 2)
                        {
                            _state = State.Save3Label;
                        }

                        _state = _saveID == 2 ? State.CancelLabel : State.Save3Label;
                    }
                    break;
                case State.Save3Label:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        copyFileId = 2;
                        CopyCheck();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        selected = true;

                        ExitCopy();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;

                        _state = _saveID == 1 ? State.Save1Label : State.Save2Label;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;

                        _state = State.CancelLabel;
                    }
                    break;
                case State.CancelLabel:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
                    {
                        selected = true;

                        ExitCopy();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;

                        _state = _saveID == 2 ? State.Save2Label : State.Save3Label;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;
                    }
                    break;
            }

            if (moved)
            {
                SoundManager.PlaySoundEffect("menu_move");
            }

            if (selected)
            {
                SoundManager.PlaySoundEffect("menu_select");
            }
        }

        private void SetConfirmation()
        {
            if (GlobalState.CurrentLanguage == Language.ES && confirmState == 2)
            {
                Vector2 offset = new Vector2(0, GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 2);
                _yesLabel.Position += offset;
                _noLabel.Position += offset;
            }
            _confirmLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 13 + confirmState * 3));
            _yesLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 15 + confirmState * 3));
            _noLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 14 + confirmState * 3));
        }

        private void ExitConfirmation()
        {
            _state = State.NewGame;

            confirmState = 0;

            SetLabels();
            _copyLabel.IsVisible = true;
        }

        private void DeletedSave()
        {
            _state = State.Game;

            _gameLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 12));

            _gameLabel.IsVisible = true;
            _newGameLabel.IsVisible = false;
            _copyLabel.IsVisible = false;

            _confirmLabel.IsVisible = false;
            _yesLabel.IsVisible = false;
            _noLabel.IsVisible = false;

            _timeLabel.IsVisible = false;
            _deathLabel.IsVisible = false;
            _cardLabel.IsVisible = false;

            _showHealth = false;

            File.Delete(Path.GetFullPath($"Save_{_saveID + 1}.dat"));

            Save = null;
        }

        private void EnterCopy()
        {
            _state = _saveID == 0 ? State.Save2Label : State.Save1Label;

            confirmState = 0;

            _copyToLabel.IsVisible = true;
            _save1Label.IsVisible = true;
            _save2Label.IsVisible = true;
            _save3Label.IsVisible = true;
            _cancelLabel.IsVisible = true;

            _gameLabel.IsVisible = false;
            _newGameLabel.IsVisible = false;
            _copyLabel.IsVisible = false;

            _confirmLabel.IsVisible = false;
            _noLabel.IsVisible = false;
            _yesLabel.IsVisible = false;

            _showHealth = false;

            _copyMode = true;
        }

        private void ExitCopy()
        {
            _state = State.Copy;

            confirmState = 0;

            _copyToLabel.IsVisible = false;
            _save1Label.IsVisible = false;
            _save2Label.IsVisible = false;
            _save3Label.IsVisible = false;
            _cancelLabel.IsVisible = false;

            _gameLabel.IsVisible = true;
            _newGameLabel.IsVisible = true;
            _copyLabel.IsVisible = true;

            _confirmLabel.IsVisible = false;
            _noLabel.IsVisible = false;
            _yesLabel.IsVisible = false;

            _showHealth = true;

            _copyMode = false;
        }

        private void CopyCheck()
        {
            SoundManager.PlaySoundEffect("menu_select");

            if (GlobalState.Save.GetSave($"{GameConstants.SavePath}Saves/Save_{copyFileId + 1}.dat") == null)
            {
                DoCopy();
            }
            else
            {
                SetConfirmation();

                _copyToLabel.IsVisible = false;
                _save1Label.IsVisible = false;
                _save2Label.IsVisible = false;
                _save3Label.IsVisible = false;
                _cancelLabel.IsVisible = false;

                _confirmLabel.IsVisible = true;
                _yesLabel.IsVisible = true;
                _noLabel.IsVisible = true;

                _state = State.NoLabel;
            }
        }

        private void DoCopy()
        {
            Save.SaveTo(copyFileId);

            RefreshSaves = true;

            ExitCopy();

        }

        public void SetLabels()
        {
            var xOffset = GlobalState.CurrentLanguage switch
            {
                Language.ES => 4,
                Language.IT => 4,
                Language.PT_BR => 6,
                _ => 0,
            };

            float x = 60 + xOffset;
            float y = 28 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 4 + (GlobalState.CurrentLanguage == Language.ZH_CN ? 2 : 0);

            Color color = new Color(116, 140, 144);
            Color colorSelected = Color.White;

            _gameLabel = new UILabel(new Vector2(x, y), false, DialogueManager.GetDialogue("misc", "any", "title", SaveExists ? 11 : 12), color);
            _newGameLabel = new UILabel(new Vector2(x, y + yStep), false, DialogueManager.GetDialogue("misc", "any", "title", 12), color)
            {
                IsVisible = SaveExists
            };
            _copyLabel = new UILabel(new Vector2(x, y + yStep * 2), false, DialogueManager.GetDialogue("misc", "any", "title", 25), color)
            {
                IsVisible = SaveExists
            };

            _confirmLabel = new UILabel(new Vector2(x, y), false, "", color)
            {
                IsVisible = false
            };
            _noLabel = new UILabel(new Vector2(x, y + yStep), false, "", color)
            {
                IsVisible = false
            };
            _yesLabel = new UILabel(new Vector2(x, y + yStep * 2), false, "", color)
            {
                IsVisible = false
            };

            _copyToLabel = new UILabel(new Vector2(x, y), false, DialogueManager.GetDialogue("misc", "any", "title", 27), color)
            {
                IsVisible = false
            };

            string save = DialogueManager.GetDialogue("misc", "any", "title", 24);

            _save1Label = new UILabel(new Vector2(x, y + yStep), false, save + 1, _saveID == 0 ? colorSelected : color)
            {
                IsVisible = false
            };
            _save2Label = new UILabel(new Vector2(x, y + yStep * 2), false, save + 2, _saveID == 1 ? colorSelected : color)
            {
                IsVisible = false
            };
            _save3Label = new UILabel(new Vector2(x, y + yStep * 3), false, save + 3, _saveID == 2 ? colorSelected : color)
            {
                IsVisible = false
            };
            _cancelLabel = new UILabel(new Vector2(x, y + yStep * 4), false, DialogueManager.GetDialogue("misc", "any", "title", 26), color)
            {
                IsVisible = false
            };

            _timeLabel = new UILabel(new Vector2(x, y + yStep * 7), false, new TimeSpan(Save?.playtime ?? 0).ToString(@"hh\:mm\:ss"))
            {
                IsVisible = SaveExists
            };

            _deathLabel = new UILabel(new Vector2(x, y + yStep * 8), false, $"{Save?.deaths ?? 0} " + DialogueManager.GetDialogue("misc", "any", "title", 22))
            {
                IsVisible = SaveExists
            };

            _cardLabel = new UILabel(new Vector2(x, y + yStep * 9), false, $"{Save?.inventory.CardCount ?? 0} " + DialogueManager.GetDialogue("misc", "any", "title", 23))
            {
                IsVisible = SaveExists
            };

            healthBar = new HealthBar(new Vector2(128 + xOffset, y + yStep * 5), Save?.current_health, Save?.max_health);

            SetConfirmation();
        }

        private void SetSelectorPos()
        {
            selector.Position = _state switch
            {
                State.Game => _gameLabel.Position,
                State.NewGame => _newGameLabel.Position,
                State.YesLabel => _yesLabel.Position,
                State.NoLabel => _noLabel.Position,
                State.Copy => _copyLabel.Position,
                State.Save1Label => _save1Label.Position,
                State.Save2Label => _save2Label.Position,
                State.Save3Label => _save3Label.Position,
                State.CancelLabel => _cancelLabel.Position,
                _ => Vector2.Zero,
            };
            selector.Position -= new Vector2(selector.sprite.Width, -2 - CursorOffset);
        }
    }
}
