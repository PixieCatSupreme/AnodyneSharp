using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using AnodyneSharp.UI.PauseMenu.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.States.MenuSubstates
{
    public class ConfigSubstate : Substate
    {
        private enum ConfigState
        {
            KeybindsLabel,
            SetBgmLabel,
            SetSfxLabel,
            AutosaveLabel,
            ResolutionLabel,
            ScalingLabel,
            LanguageLabel
        }

        private List<(UILabel label, UIOption option)> options;
        private int _state = 0;
        private int _lastState = 0;

        private UIOption _selectedOption;

        private bool _isInMainMenu;

        public ConfigSubstate() : this(false) { }

        public ConfigSubstate(bool isInMainMenu)
        {
            _isInMainMenu = isInMainMenu;

            SetLabels();
        }

        public override void GetControl()
        {
            base.GetControl();
            _lastState = _state;

            SetSelectorPos();
        }

        protected override void OnExit()
        {
            GlobalState.settings.Save();
        }

        public override void Update()
        {
            if (_lastState != _state)
            {
                _lastState = _state;
                SetSelectorPos();
                SoundManager.PlaySoundEffect("menu_move");
            }

            base.Update();
        }

        public override void HandleInput()
        {
            if (_selectedOption != null)
            {
                _selectedOption.Update();

                if (_selectedOption.Exit)
                {
                    _selector.visible = true;

                    _selectedOption.LoseControl();
                    _selectedOption.Exit = false;
                    _selectedOption = null;
                    _state = _lastState;
                    SetSelectorPos();
                }
            }
            else
            {
                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                {
                    if (_state == 0)
                    {
                        return;
                    }

                    _state--;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                {
                    if (_state >= options.Count - 1)
                    {
                        return;
                    }

                    _state++;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
                {
                    SetSettingsState();
                }
                else
                {
                    base.HandleInput();
                }
            }
        }


        public override void DrawUI()
        {
            foreach (var (label, option) in options)
            {
                label.Draw();
                option.Draw();
            }

            if (_selectedOption == null)
            {
                _selector.Draw();
            }
        }

        private void SetLabels()
        {
            float x = 60 + (_isInMainMenu ? 0 : (GlobalState.CurrentLanguage == Language.IT ? 4 : 9));
            float y = 28 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset;

            Color color = _isInMainMenu ? new Color(116, 140, 144) : Color.White;

            var keybindsLabel = new UILabel(new Vector2(x, y), true, DialogueManager.GetDialogue("misc", "any", "config", 1), color);

            var bgmLabel = new UILabel(new Vector2(x, keybindsLabel.Position.Y + yStep * 2), true, "BGM", color, forceEnglish: true);
            var sfxLabel = new UILabel(new Vector2(x, bgmLabel.Position.Y + 12), true, "SFX", color, forceEnglish: true);

            var autosaveLabel = new UILabel(new Vector2(x, sfxLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 3), color);
            var resolutionLabel = new UILabel(new Vector2(x, autosaveLabel.Position.Y + yStep * 4), true, DialogueManager.GetDialogue("misc", "any", "config", 6), color);
            var scalingLabel = new UILabel(new Vector2(x, resolutionLabel.Position.Y + yStep * 3), true, DialogueManager.GetDialogue("misc", "any", "config", 16), color);
            var languageLabel = new UILabel(new Vector2(x, scalingLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 17), color);

            var musicSlider = new AudioSlider(new Vector2(bgmLabel.Position.X + bgmLabel.Writer.WriteArea.Width - 5, bgmLabel.Position.Y), GlobalState.settings.music_volume_scale, 0f, 1f, 0.1f, _isInMainMenu)
            {
                ValueChangedEvent = BgmValueChanged
            };

            var sfxSlider = new AudioSlider(new Vector2(sfxLabel.Position.X + sfxLabel.Writer.WriteArea.Width - 5, sfxLabel.Position.Y), GlobalState.settings.sfx_volume_scale, 0f, 1f, 0.1f, _isInMainMenu)
            {
                ValueChangedEvent = SfxValueChanged
            };

            Vector2 autosavePos = Vector2.Zero;

            if (GlobalState.CurrentLanguage == Language.ZH_CN)
            {
                autosavePos = new Vector2(x + 48, autosaveLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT + 5);
            }
            else if (GlobalState.CurrentLanguage == Language.IT)
            {
                autosavePos = new Vector2(x + 84, autosaveLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT + 2);
            }
            else
            {
                autosavePos = new Vector2(x + 78, autosaveLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT + 2);
            }

            var autosaveSetter = new CheckBox(autosavePos, GlobalState.settings.autosave_on, _isInMainMenu)
            {
                ValueChangedEvent = AutoSaveValueChanged
            };

            var languageSetter = new TextSelector(new Vector2(x + languageLabel.Writer.WriteArea.Width - 8, languageLabel.Position.Y + GameConstants.LineOffset), GlobalState.CurrentLanguage == Language.ZH_CN ? 40 : 30, (int)GlobalState.CurrentLanguage, true, Enum.GetNames(GlobalState.CurrentLanguage.GetType()).Select(s => s.ToLower().Replace('_', '-')).ToArray())
            {
                ValueChangedEvent = LanguageValueChanged
            };

            options = new()
            {
                (keybindsLabel, new SubstateOption<ControlsSubstate>()),
                (bgmLabel, musicSlider),
                (sfxLabel, sfxSlider),
                (autosaveLabel, autosaveSetter),
                //TODO: give these their own menu
                (resolutionLabel, new SubstateOption<ControlsSubstate>()),
                (scalingLabel, new SubstateOption<ControlsSubstate>()),
                (languageLabel, languageSetter)
            };

            _state = _lastState;
        }

        private void SetSettingsState()
        {
            _selector.visible = false;
            _selectedOption = options[_state].option;
            _selectedOption.GetControl();

            SoundManager.PlaySoundEffect("menu_select");

            SetSelectorPos();
        }

        private void SetSelectorPos()
        {
            _selector.Position = options[_state].label.Position - new Vector2(_selector.sprite.Width, -2);

            if (!options[_state].label.ForcedEnglish)
            {
                _selector.Position.Y += CursorOffset;
            }
        }

        private void BgmValueChanged(float value, int index)
        {
            GlobalState.settings.music_volume_scale = value;
            SoundManager.SetSongVolume();
        }

        private void SfxValueChanged(float value, int index)
        {
            GlobalState.settings.sfx_volume_scale = value;
        }

        private void AutoSaveValueChanged(bool value)
        {
            GlobalState.settings.autosave_on = value;
        }

        private void LanguageValueChanged(string value, int index)
        {
            Language lang = (Language)index;
            GlobalState.settings.language = lang;
            DialogueManager.Reload();

            SetLabels();
            GlobalState.RefreshLabels = true;
        }
    }
}
