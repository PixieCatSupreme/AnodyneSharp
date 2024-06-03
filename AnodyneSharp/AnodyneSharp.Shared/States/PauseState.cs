using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.States.MainMenu;
using AnodyneSharp.States.MenuSubstates;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.States
{
    public class PauseState : State
    {

        private const int CheatCounterMax =
#if DEBUG
            2;
#else
            20;
#endif

        private static int _state = 0;

        private int LastAllowedState => substates.Count - 1;
        private bool InCheatState => _state == substates.Count;

        public static void Reset()
        {
            _state = 0;
        }

        private Texture2D _bg;

        List<(UILabel label, Func<Substate> create)> substates;

        private UILabel _playtimeLabel;
        private UILabel _inputLabel;

        private PauseMenuSelector _selector;
        private int _lastState;
        private int cheat_counter;

        private bool _inSubstate;

        private Substate _substate;

        public PauseState()
        {
            _bg = ResourceManager.GetTexture("menu_bg", true);
            _selector = new PauseMenuSelector(new Vector2(0, 30));

            _lastState = _state;

            SetLabels();
            StateChanged();

            UpdateEntities = false;
        }

        public override void Update()
        {
            if (GlobalState.CUR_HEALTH == 0)
            {
                Exit = true;
                return;
            }

            if (GlobalState.RefreshLabels)
            {
                GlobalState.RefreshLabels = false;
                SetLabels();
            }

            _playtimeLabel.SetText(GlobalState.PlayTime.ToString(@"hh\:mm\:ss"));

            if (KeyInput.ControllerModeChanged)
            {
                _inputLabel.SetText($"{DialogueManager.GetDialogue("misc", "any", "secrets", 14)} {DialogueManager.GetDialogue("misc", "any", "secrets", 15)}");
            }

            _selector.Update();
            _selector.PostUpdate();

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Pause) || (!_inSubstate && KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel)))
            {
                SoundManager.PlayPitchedSoundEffect("pause_sound", -0.1f);
                Exit = true;
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
                    _selector.Play("flash");
                    if (GlobalState.WARP) //exit pause state if substate triggered a warp
                    {
                        Exit = true;
                        return;
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

            SpriteDrawer.DrawSprite(_bg, new Vector2(0, GameConstants.HEADER_HEIGHT), Z: DrawingUtilities.GetDrawingZ(DrawOrder.PAUSE_BG));
            _selector.Draw();

            foreach (var (label, _) in substates)
            {
                label.Draw();
            }

            _playtimeLabel.Draw();
            _inputLabel.Draw();

            _substate.DrawUI();
        }

        private void StateChanged()
        {
            _lastState = _state;
            _selector.Position = new Vector2(0, 30 + _state * 16);

            if (_state == substates.Count)
            {
                _substate = new CheatzSubstate();
            }
            else
            {
                _substate = substates[_state].create();
            }
        }

        private void BrowseInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) || KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
            {
                SoundManager.PlaySoundEffect("menu_select");
                _inSubstate = true;
                _selector.Play("inactive");
                _substate.GetControl();
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                cheat_counter = 0;

                if (_state == 0)
                {
                    return;
                }

                SoundManager.PlaySoundEffect("menu_move");
                _state--;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {
                if (InCheatState)
                {
                    return;
                }

                if (_state == LastAllowedState)
                {
                    SoundManager.PlaySoundEffect("menu_move");
                    cheat_counter++;

                    if (cheat_counter == CheatCounterMax)
                    {
                        cheat_counter = 0;
                        _state++;
                    }

                    return;
                }

                SoundManager.PlaySoundEffect("menu_move");
                _state++;
            }
        }

        private void SetLabels()
        {

            float x = 10f;
            float startY = GameConstants.HEADER_HEIGHT - GameConstants.LineOffset + 11 + (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = (GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset) * 2;

            substates = GetLabels().Select((state, index) => (new UILabel(new(x, startY + yStep * index), true, state.name), state.create)).ToList();

            _playtimeLabel = new UILabel(new Vector2(1, 154), true, "00:00:00", forceEnglish: true);

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
            _inputLabel = new UILabel(inputPos, false, $"{DialogueManager.GetDialogue("misc", "any", "secrets", 14)} {DialogueManager.GetDialogue("misc", "any", "secrets", 15)}", new Color(143, 153, 176, 255));
        }

        private List<(string name, Func<Substate> create)> GetLabels()
        {
            List<(string name, Func<Substate> create)> ret = new()
            {
                (DialogueManager.GetDialogue("misc", "any", "map", 0), () => new MapSubstate()),
                (DialogueManager.GetDialogue("misc", "any", "items", 0), () => new EquipSubstate()),
                (DialogueManager.GetDialogue("misc", "any", "cards", 0), () => new CardSubstate()),
                (DialogueManager.GetDialogue("misc", "any", "save", 0), () => new SaveSubstate()),
                (DialogueManager.GetDialogue("misc", "any", "config", 0), () => new ConfigSubstate()),
            };

            if (GlobalState.inventory.UnlockedSecretz)
            {
                ret.Add(("???", () => new SecretSubstate()));
            }

            //TODO: allow for modding

            return ret;
        }
    }
}
