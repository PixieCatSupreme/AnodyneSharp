﻿using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.States.MenuSubstates;
using AnodyneSharp.States.MenuSubstates.MainMenu;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.MainMenu
{
    public class MainMenuState : State
    {
        private enum MenuState
        {
            Save1,
            Save2,
            Save3,
            Settings
        }

        private static MenuState _state;

        private UILabel _save1Label;
        private UILabel _save2Label;
        private UILabel _save3Label;
        private UILabel _settingsLabel;

        private MenuSelector _selector;
        private MenuState _lastState;

        private bool _inSubstate;

        private Substate _substate;

        public MainMenuState()
        {
            _selector = new MenuSelector();
            _selector.Play("enabledRight");

            _lastState = _state;

            SetLabels();
            StateChanged();

            UpdateEntities = false;
        }

        public override void Update()
        {
            base.Update();

            _selector.Update();
            _selector.PostUpdate();

            if (!_inSubstate && KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
            {
                SoundManager.PlayPitchedSoundEffect("pause_sound", -0.1f);
                ChangeStateEvent(AnodyneGame.GameState.TitleScreen);
            }
            else if (!_inSubstate)
            {
                BrowseInput();
            }
            else
            {
                _substate.HandleInput();

                if (_substate.Exit)
                {
                    _inSubstate = false;
                    _substate.Exit = false;
                    _selector.Play("enabledRight");

                    if (_substate is FileSubstate s && s.LoadedSave)
                    {
                        ChangeStateEvent(s.NewSave ? AnodyneGame.GameState.Intro : AnodyneGame.GameState.Game);
                    }
                }
            }

            _substate.Update();

            if (_lastState != _state)
            {
                StateChanged();
            }
        }


        public override void DrawUI()
        {
            base.DrawUI();

            _selector.Draw();

            _save1Label.Draw();
            _save2Label.Draw();
            _save3Label.Draw();
            _settingsLabel.Draw();

            _substate.DrawUI();
        }

        private void BrowseInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
            {
                SoundManager.PlaySoundEffect("menu_select");
                _inSubstate = true;
                _selector.Play("disabledRight");
                _substate.GetControl();
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                if (_state == MenuState.Save1)
                {
                    SoundManager.PlaySoundEffect("menu_move");
                    return;
                }

                SoundManager.PlaySoundEffect("menu_move");
                _state--;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {
                if (_state == MenuState.Settings)
                {
                    SoundManager.PlaySoundEffect("menu_move");
                    return;
                }

                SoundManager.PlaySoundEffect("menu_move");
                _state++;
            }
        }


        private void StateChanged()
        {
            _lastState = _state;
            _selector.Position = new Vector2(2, 34 + (int)_state * 16);

            _substate = _state switch
            {
                MenuState.Save1 => new FileSubstate(0),
                MenuState.Save2 => new FileSubstate(1),
                MenuState.Save3 => new FileSubstate(2),
                MenuState.Settings => new ConfigSubstate(true),
                _ => new Substate(),
            };
        }

        private void SetLabels()
        {
            float x = 10f;
            float startY = GameConstants.HEADER_HEIGHT - GameConstants.LineOffset + 11 + (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = (GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset) * 2;

            Color color = new Color(116, 140, 144);

            _save1Label = new UILabel(new Vector2(x, startY), false, color);
            _save2Label = new UILabel(new Vector2(x, startY + yStep), false, color);
            _save3Label = new UILabel(new Vector2(x, startY + yStep * 2), false, color);
            _settingsLabel = new UILabel(new Vector2(x, startY + yStep * 3), false, color);



            _save1Label.Initialize();
            _save2Label.Initialize();
            _save3Label.Initialize();
            _settingsLabel.Initialize();

            _save1Label.SetText("Save 1");
            _save2Label.SetText("Save 2");
            _save3Label.SetText("Save 3");
            _settingsLabel.SetText(DialogueManager.GetDialogue("misc", "any", "config", 0));
        }
    }
}
