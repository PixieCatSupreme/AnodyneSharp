using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
            NoLabel
        }

        public bool LoadedSave { get; private set; }

        private State _state;
        private State _lastState;

        private int _saveID;
        private bool _saveExists;

        private int confirmState;

        private UILabel _gameLabel;
        private UILabel _newGameLabel;
        private UILabel _confirmLabel;
        private UILabel _yesLabel;
        private UILabel _noLabel;

        private HealthBar healthBar;

        private UILabel _timeLabel;
        private UILabel _deathLabel;
        private UILabel _cardLabel;

        public FileSubstate(int saveID)
        {
            _saveID = saveID;

            confirmState = 0;

            _saveExists = true;

            SetLabels();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            _gameLabel.Draw();
            _newGameLabel.Draw();
            _confirmLabel.Draw();
            _yesLabel.Draw();
            _noLabel.Draw();

            _timeLabel.Draw();
            _deathLabel.Draw();
            _cardLabel.Draw();


            //Healthbar has no visible
            if (_saveExists)
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

            SetSelectorPos();
        }

        public override void HandleInput()
        {
            bool moved = false;

            switch (_state)
            {
                case State.Game:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        if (_saveExists)
                        {
                            //TODO load save
                        }
                        else
                        {
                            //TODO create save
                        }

                        LoadedSave = true;

                        ExitSubState();
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;
                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                    {
                        moved = true;

                        if (_saveExists)
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
                        SoundManager.PlaySoundEffect("menu_select");

                        SetConfirmation();

                        _confirmLabel.IsVisible = true;
                        _yesLabel.IsVisible = true;
                        _noLabel.IsVisible = true;

                        _gameLabel.IsVisible = false;
                        _newGameLabel.IsVisible = false;

                    }
                    else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                    {
                        moved = true;

                        _state = State.Game;
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
                case State.YesLabel:
                    if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                    {
                        SoundManager.PlaySoundEffect("menu_select");

                        if (confirmState == 2)
                        {
                            DeletedSave();
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
                        ExitConfirmation();
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
                        ExitConfirmation();
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
            }

            if (moved)
            {
                SoundManager.PlaySoundEffect("menu_move");
            }
        }

        private void SetConfirmation()
        {
            _confirmLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 13 + confirmState * 3));
            _yesLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 15 + confirmState * 3));
            _noLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 14 + confirmState * 3));
        }

        private void ExitConfirmation()
        {
            SoundManager.PlaySoundEffect("menu_select");

            _state = State.NewGame;

            confirmState = 0;

            _confirmLabel.IsVisible = false;
            _yesLabel.IsVisible = false;
            _noLabel.IsVisible = false;

            _gameLabel.IsVisible = true;
            _newGameLabel.IsVisible = true;
        }

        private void DeletedSave()
        {
            _state = State.Game;

            _gameLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 12));

            _gameLabel.IsVisible = true;
            _newGameLabel.IsVisible = false;

            _confirmLabel.IsVisible = false;
            _yesLabel.IsVisible = false;
            _noLabel.IsVisible = false;

            _timeLabel.IsVisible = false;
            _deathLabel.IsVisible = false;
            _cardLabel.IsVisible = false;

            _saveExists = false;
        }

        private void SetLabels()
        {
            float x = 60;
            float y = 28 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 2;

            Color color = new Color(116, 140, 144);

            _gameLabel = new UILabel(new Vector2(x, y), false, color);
            _newGameLabel = new UILabel(new Vector2(x, y + yStep), false, color)
            {
                IsVisible = _saveExists
            };
            _confirmLabel = new UILabel(new Vector2(x, y), false, color)
            {
                IsVisible = false
            };
            _noLabel = new UILabel(new Vector2(x, y + yStep), false, color)
            {
                IsVisible = false
            };
            _yesLabel = new UILabel(new Vector2(x, y + yStep * 2), false, color)
            {
                IsVisible = false
            };

            _timeLabel = new UILabel(new Vector2(x, y + yStep * 6), false)
            {
                IsVisible = _saveExists
            };

            _deathLabel = new UILabel(new Vector2(x, y + yStep * 7), false)
            {
                IsVisible = _saveExists
            };

            _cardLabel = new UILabel(new Vector2(x, y + yStep * 8), false)
            {
                IsVisible = _saveExists
            };

            healthBar = new HealthBar(new Vector2(128, y + yStep * 4));


            _gameLabel.Initialize();
            _newGameLabel.Initialize();
            _confirmLabel.Initialize();
            _yesLabel.Initialize();
            _noLabel.Initialize();

            _timeLabel.Initialize();
            _deathLabel.Initialize();
            _cardLabel.Initialize();

            _gameLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", _saveExists ? 11 : 12));
            _newGameLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 12));

            _timeLabel.SetText("00:00:00");
            _deathLabel.SetText($"{GlobalState.DeathCount} " + DialogueManager.GetDialogue("misc", "any", "title", 22));
            _cardLabel.SetText($"{InventoryManager.CardCount} " + DialogueManager.GetDialogue("misc", "any", "title", 23));

            SetConfirmation();
        }

        private void SetSelectorPos()
        {
            switch (_state)
            {
                case State.Game:
                    _selector.Position = _gameLabel.Position;
                    break;
                case State.NewGame:
                    _selector.Position = _newGameLabel.Position;
                    break;
                case State.YesLabel:
                    _selector.Position = _yesLabel.Position;
                    break;
                case State.NoLabel:
                    _selector.Position = _noLabel.Position;
                    break;
            }

            _selector.Position -= new Vector2(_selector.sprite.Width, -2 - CursorOffset);
        }
    }
}
