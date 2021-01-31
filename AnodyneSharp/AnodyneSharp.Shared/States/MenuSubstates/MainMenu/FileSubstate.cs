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
            NoLabel
        }

        public bool LoadedSave { get; private set; }
        public bool NewSave { get; private set; }

        private State _state;
        private State _lastState;

        private int _saveID;
        private bool SaveExists => Save != null;

        public GlobalState.Save Save { get; private set; }

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

            Save = GlobalState.Save.getSave($"Save_{saveID + 1}.dat");

            confirmState = 0;

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
            if (SaveExists)
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
                        GlobalState.CurrentSaveGame = _saveID;
                        
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

            File.Delete(Path.GetFullPath($"Save_{_saveID + 1}.dat"));

            Save = null;
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
                IsVisible = SaveExists
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
                IsVisible = SaveExists
            };

            _deathLabel = new UILabel(new Vector2(x, y + yStep * 7), false)
            {
                IsVisible = SaveExists
            };

            _cardLabel = new UILabel(new Vector2(x, y + yStep * 8), false)
            {
                IsVisible = SaveExists
            };

            healthBar = new HealthBar(new Vector2(128, y + yStep * 4),Save?.current_health, Save?.max_health);


            _gameLabel.Initialize();
            _newGameLabel.Initialize();
            _confirmLabel.Initialize();
            _yesLabel.Initialize();
            _noLabel.Initialize();

            _timeLabel.Initialize();
            _deathLabel.Initialize();
            _cardLabel.Initialize();

            _gameLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", SaveExists ? 11 : 12));
            _newGameLabel.SetText(DialogueManager.GetDialogue("misc", "any", "title", 12));

            _timeLabel.SetText(new TimeSpan(Save?.playtime ?? 0).ToString(@"hh\:mm\:ss"));
            _deathLabel.SetText($"{Save?.deaths ?? 0} " + DialogueManager.GetDialogue("misc", "any", "title", 22));
            _cardLabel.SetText($"{Save?.inventory.CardCount ?? 0} " + DialogueManager.GetDialogue("misc", "any", "title", 23));

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
