using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.States.MenuSubstates;
using AnodyneSharp.States.MenuSubstates.MainMenu;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.States.MainMenu
{
    public class MainMenuState : State
    {
        private static int _state = 0;

        List<(UILabel label, Func<Substate> create)> substates;

        private UILabel _inputLabel;

        private MenuSelector _selector;
        private int _lastState = _state;

        private bool _inSubstate;

        private Substate _substate;

        private float xOffset;

        public MainMenuState()
        {
            _selector = new MenuSelector();
            _selector.Play("enabledRight");

            UpdateEntities = false;

            SetLabels();
            StateChanged();
        }

        public override void Update()
        {
            base.Update();

            if (GlobalState.RefreshLabels)
            {
                GlobalState.RefreshLabels = false;
                SetLabels();
            }

            if (KeyInput.ControllerModeChanged)
            {
                _inputLabel.SetText($"{DialogueManager.GetDialogue("misc", "any", "secrets", 14)} {DialogueManager.GetDialogue("misc", "any", "secrets", 15)}");
            }

            _selector.Update();
            _selector.PostUpdate();

            _substate.Update();

            if (!_inSubstate && KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
            {
                SoundManager.PlayPitchedSoundEffect("pause_sound", -0.1f);
                GlobalState.GameState.SetState<TitleState>();
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
                }
            }

            if (_lastState != _state)
            {
                StateChanged();
            }
        }


        public override void DrawUI()
        {
            base.DrawUI();

            _selector.Draw();

            foreach (var (label, _) in substates)
            {
                label.Draw();
            }

            _inputLabel.Draw();

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
                if (_state == 0)
                {
                    SoundManager.PlaySoundEffect("menu_move");
                    return;
                }

                SoundManager.PlaySoundEffect("menu_move");
                _state--;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {
                if (_state == substates.Count-1)
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
            _selector.Position = new Vector2(2 + xOffset, 34 + _state * 16);

            _substate = substates[_state].create();
        }

        private void SetLabels()
        {
            xOffset = GlobalState.CurrentLanguage switch
            {
                Language.ES => -2,
                Language.IT => -2,
                Language.PT_BR => -2,
                _ => 0,
            };

            float x = 10f + xOffset;
            float startY = GameConstants.HEADER_HEIGHT - GameConstants.LineOffset + 11 + (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = (GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset) * 2;

            Color color = new Color(116, 140, 144);

            substates = GetLabels().Select((state, index) => (new UILabel(new(x, startY + yStep * index), false, state.name, color), state.create)).ToList();

            Vector2 inputPos = Vector2.Zero;
            if (GlobalState.CurrentLanguage == Language.ZH_CN)
            {
                inputPos = new Vector2(2, 168 - GameConstants.LineOffset + 1);
            }
            else
            {
                inputPos = new Vector2(2, 168);
                if (GlobalState.CurrentLanguage == Language.KR)
                {
                    inputPos.Y -= 1;
                }
            }
            _inputLabel = new UILabel(inputPos, false, $"{DialogueManager.GetDialogue("misc", "any", "secrets", 14)} {DialogueManager.GetDialogue("misc", "any", "secrets", 15)}", color);
        }

        private List<(string name, Func<Substate> create)> GetLabels()
        {
            List<(string name, Func<Substate> create)> ret = new()
            {
                ("1", () => new FileSubstate(0)),
                ("2", () => new FileSubstate(1)),
                ("3", () => new FileSubstate(2)),
                (DialogueManager.GetDialogue("misc", "any", "config", 0), () => new ConfigSubstate(true)),
            };

            //TODO: allow for modding

            return ret;
        }
    }
}
