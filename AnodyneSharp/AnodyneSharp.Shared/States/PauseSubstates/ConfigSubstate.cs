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
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class ConfigSubstate : PauseSubstate
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

        private OptionSelector _selectedOption;

        private ConfigState _lastState;

        private AudioSlider _musicSlider;
        private AudioSlider _sfxSlider;

        public ConfigSubstate()
        {
            float x = 69;
            float y = 28;

            _keybindsLabel = new UILabel(new Vector2(x, y), true);

            _bgmLabel = new UILabel(new Vector2(x, _keybindsLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 2), true);
            _sfxLabel = new UILabel(new Vector2(x, _bgmLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT +4), true);

            _autosaveLabel = new UILabel(new Vector2(x, _sfxLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 2), true);
            _resolutionLabel = new UILabel(new Vector2(x, _autosaveLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 4), true);
            _scalingLabel = new UILabel(new Vector2(x, _resolutionLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 3), true);
            _languageLabel = new UILabel(new Vector2(x, _scalingLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 2), true);

            _keybindsLabel.Initialize();

            _bgmLabel.Initialize();
            _sfxLabel.Initialize();

            _autosaveLabel.Initialize();
            _resolutionLabel.Initialize();
            _scalingLabel.Initialize();
            _languageLabel.Initialize();

            _keybindsLabel.SetText("Set keybinds");

            _bgmLabel.SetText("BGM");
            _sfxLabel.SetText("SFX");

            _autosaveLabel.SetText("Autosave at\ncheckpoints:");
            _resolutionLabel.SetText("Resolution:");
            _scalingLabel.SetText("Scaling:");
            _languageLabel.SetText("Language:");

            _musicSlider = new AudioSlider(new Vector2(_bgmLabel.Position.X + _bgmLabel.Writer.WriteArea.Width -5, _bgmLabel.Position.Y), GlobalState.music_volume_scale, 0f, 1f, 0.1f)
            {
                ValueChangedEvent = BgmValueChanged
            };

            _sfxSlider = new AudioSlider(new Vector2(_sfxLabel.Position.X + _sfxLabel.Writer.WriteArea.Width -5, _sfxLabel.Position.Y), GlobalState.sfx_volume_scale, 0f, 1f, 0.1f)
            {
                ValueChangedEvent = SfxValueChanged
            };

            _autosaveSetter = new TextSelector(new Vector2(x + 16, _autosaveLabel.Position.Y + GameConstants.FONT_LINE_HEIGHT * 2.5f), 40, GlobalState.autosave_on? 1 : 0, "Off", "On")
            {
                ValueChangedEvent = AutoSaveValueChanged
            };
        }

        public override void GetControl()
        {
            base.GetControl();
            _lastState = _state;

            SetSelectorPos();
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
                if (KeyInput.JustPressedKey(Keys.Up))
                {
                    if (_state == ConfigState.KeybindsLabel)
                    {
                        return;
                    }

                    _state--;
                }
                else if (KeyInput.JustPressedKey(Keys.Down))
                {
                    if (_state >= ConfigState.LanguageLabel)
                    {
                        return;
                    }

                    _state++;
                }
                else if (KeyInput.JustPressedKey(Keys.C))
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

            _selector.Draw();
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
                //case ConfigState.LanguageLabel:
                //    _state = ConfigState.SettingLanguage;
                //    break;
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
            switch (_state)
            {
                case ConfigState.KeybindsLabel:
                    _selector.Position = _keybindsLabel.Position;
                    break;
                case ConfigState.SetBgmLabel:
                    _selector.Position = _bgmLabel.Position;
                    break;
                case ConfigState.SetSfxLabel:
                    _selector.Position = _sfxLabel.Position;
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

            _selector.Position -= new Vector2(_selector.frameWidth, -2);
        }

        private void BgmValueChanged(string value, int index)
        {
            if (float.TryParse(value, out float volume))
            {
                GlobalState.music_volume_scale = volume;
                SoundManager.SetSongVolume();
            }
        }

        private void SfxValueChanged(string value, int index)
        {
            if (float.TryParse(value, out float volume))
            {
                GlobalState.sfx_volume_scale = volume;
            }
        }

        private void AutoSaveValueChanged(string value, int index)
        {
            //TODO localization

            GlobalState.autosave_on = value == "On";
        }
    }
}
