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

        private static ConfigState _state;

        private UILabel _keybindsLabel;


        private UILabel _bgmLabel;
        private UILabel _sfxLabel;

        private UILabel _autosaveLabel;
        private UILabel _resolutionLabel;
        private UILabel _scalingLabel;
        private UILabel _languageLabel;

        private TextSelector _autosaveSetter;

        private TextSelector _languageSetter;

        private OptionSelector _selectedOption;

        private ConfigState _lastState;

        private AudioSlider _musicSlider;
        private AudioSlider _sfxSlider;

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

                    _selectedOption.Enabled = false;
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
                    if (_state == ConfigState.KeybindsLabel)
                    {
                        return;
                    }

                    _state--;
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
                {
                    if (_state >= ConfigState.LanguageLabel)
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
            _keybindsLabel.Draw();

            _bgmLabel.Draw();
            _sfxLabel.Draw();

            _autosaveLabel.Draw();
            _resolutionLabel.Draw();
            _scalingLabel.Draw();
            _languageLabel.Draw();

            _musicSlider.DrawUI();
            _sfxSlider.DrawUI();


            _autosaveSetter.DrawUI();

            _languageSetter.DrawUI();

            _selector.Draw();
        }

        private void SetLabels()
        {
            float x = 60 + (_isInMainMenu ? 0 : 9);
            float y = 28 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset;

            Color color = _isInMainMenu ? new Color(116, 140, 144) : Color.White;

            _keybindsLabel = new UILabel(new Vector2(x, y), true, DialogueManager.GetDialogue("misc", "any", "config", 1), color);

            _bgmLabel = new UILabel(new Vector2(x, _keybindsLabel.Position.Y + yStep * 2), true, "BGM", color, forceEnglish: true);
            _sfxLabel = new UILabel(new Vector2(x, _bgmLabel.Position.Y + 12), true, "SFX", color, forceEnglish: true);

            _autosaveLabel = new UILabel(new Vector2(x, _sfxLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 3),color);
            _resolutionLabel = new UILabel(new Vector2(x, _autosaveLabel.Position.Y + yStep * 4), true, DialogueManager.GetDialogue("misc", "any", "config", 6), color);
            _scalingLabel = new UILabel(new Vector2(x, _resolutionLabel.Position.Y + yStep * 3), true, DialogueManager.GetDialogue("misc", "any", "config", 16), color);
            _languageLabel = new UILabel(new Vector2(x, _scalingLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 17), color);

            _musicSlider = new AudioSlider(new Vector2(_bgmLabel.Position.X + _bgmLabel.Writer.WriteArea.Width - 5, _bgmLabel.Position.Y), GlobalState.settings.music_volume_scale, 0f, 1f, 0.1f, _isInMainMenu)
            {
                ValueChangedEvent = BgmValueChanged
            };

            _sfxSlider = new AudioSlider(new Vector2(_sfxLabel.Position.X + _sfxLabel.Writer.WriteArea.Width - 5, _sfxLabel.Position.Y), GlobalState.settings.sfx_volume_scale, 0f, 1f, 0.1f, _isInMainMenu)
            {
                ValueChangedEvent = SfxValueChanged
            };

            Vector2 autosavePos = Vector2.Zero;

            if (GlobalState.CurrentLanguage == Language.ZH_CN)
            {
                autosavePos = new Vector2(x + 44, _autosaveLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT + 5);
            }
            else
            {
                autosavePos = new Vector2(x + 16, _autosaveLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 2.5f);
            }

            _autosaveSetter = new TextSelector(autosavePos, 40, GlobalState.settings.autosave_on ? 0 : 1, DialogueManager.GetDialogue("misc", "any", "config", 4), DialogueManager.GetDialogue("misc", "any", "config", 5))
            {
                ValueChangedEvent = AutoSaveValueChanged
            };

            _languageSetter = new TextSelector(new Vector2(x + _languageLabel.Writer.WriteArea.Width - 8, _languageLabel.Position.Y + GameConstants.LineOffset), GlobalState.CurrentLanguage == Language.ZH_CN ? 40 : 30, (int)GlobalState.CurrentLanguage, Enum.GetNames(GlobalState.CurrentLanguage.GetType()).Select(s => s.ToLower().Replace('_', '-')).ToArray())
            {
                ValueChangedEvent = LanguageValueChanged
            };

            _state = _lastState;
        }

        private void SetSettingsState()
        {
            switch (_state)
            {
                //case ConfigState.KeybindsLabel:
                //    _state = ConfigState.SettingKeyBinds;
                //break;
                case ConfigState.SetBgmLabel:
                    _selectedOption = _musicSlider;
                    break;
                case ConfigState.SetSfxLabel:
                    _selectedOption = _sfxSlider;
                    break;
                case ConfigState.AutosaveLabel:
                    _selectedOption = _autosaveSetter;
                    break;
                //case ConfigState.ResolutionLabel:
                //    _state = ConfigState.SettingResolution;
                //    break;
                //case ConfigState.ScalingLabel:
                //    _state = ConfigState.SettingScale;
                //    break;
                case ConfigState.LanguageLabel:
                    _selectedOption = _languageSetter;
                    break;
                default:
                    _state = ConfigState.KeybindsLabel;
                    _selectedOption = null;
                    break;
            }

            if (_selectedOption != null)
            {
                _selector.visible = false;
                _selectedOption.Enabled = true;
            }

            SoundManager.PlaySoundEffect("menu_select");

            SetSelectorPos();
        }

        private void SetSelectorPos()
        {
            bool ignoreOffset = false;

            switch (_state)
            {
                case ConfigState.KeybindsLabel:
                    _selector.Position = _keybindsLabel.Position;
                    break;
                case ConfigState.SetBgmLabel:
                    _selector.Position = _bgmLabel.Position;
                    ignoreOffset = true;
                    break;
                case ConfigState.SetSfxLabel:
                    _selector.Position = _sfxLabel.Position;
                    ignoreOffset = true;

                    break;
                case ConfigState.AutosaveLabel:
                    _selector.Position = _autosaveLabel.Position;
                    break;
                case ConfigState.ResolutionLabel:
                    _selector.Position = _resolutionLabel.Position;
                    break;
                case ConfigState.ScalingLabel:
                    _selector.Position = _scalingLabel.Position;
                    break;
                case ConfigState.LanguageLabel:
                    _selector.Position = _languageLabel.Position;
                    break;
            }

            _selector.Position -= new Vector2(_selector.sprite.Width, -2);

            if (!ignoreOffset)
            {
                _selector.Position.Y += CursorOffset;
            }
        }

        private void BgmValueChanged(string value, int index)
        {
            if (float.TryParse(value, out float volume))
            {
                GlobalState.settings.music_volume_scale = volume;
                SoundManager.SetSongVolume();
            }
        }

        private void SfxValueChanged(string value, int index)
        {
            if (float.TryParse(value, out float volume))
            {
                GlobalState.settings.sfx_volume_scale = volume;
            }
        }

        private void AutoSaveValueChanged(string value, int index)
        {
            GlobalState.settings.autosave_on = value == DialogueManager.GetDialogue("misc", "any", "config", 4);
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
