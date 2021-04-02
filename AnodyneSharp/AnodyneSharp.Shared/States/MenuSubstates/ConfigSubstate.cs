using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.States.MenuSubstates.ConfigSubstates;
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
    public class ConfigSubstate : ListSubstate
    {
        private bool _isInMainMenu;

        public ConfigSubstate() : this(false) { }

        public ConfigSubstate(bool isInMainMenu) : base(true)
        {
            _isInMainMenu = isInMainMenu;
            SetLabels();
        }

        protected override void OnExit()
        {
            GlobalState.settings.Save();
            base.OnExit();
        }

        protected override void SetLabels()
        {
            float x = 60 + (_isInMainMenu ? 0 : (GlobalState.CurrentLanguage == Language.IT ? 4 : 9));
            float y = 28 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset;

            Color color = _isInMainMenu ? new Color(116, 140, 144) : Color.White;

            var keybindsLabel = new UILabel(new Vector2(x, y), true, DialogueManager.GetDialogue("misc", "any", "config", 1), color);

            var bgmLabel = new UILabel(new Vector2(x, keybindsLabel.Position.Y + yStep * 2), true, "BGM", color, forceEnglish: true);
            var sfxLabel = new UILabel(new Vector2(x, bgmLabel.Position.Y + 12), true, "SFX", color, forceEnglish: true);

            var autosaveLabel = new UILabel(new Vector2(x, sfxLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 3), color);

            //TODO: localize
            var graphicsLabel = new UILabel(new Vector2(x, autosaveLabel.Position.Y + yStep * 4), true, "Graphics", color, forceEnglish: true);
            
            var languageLabel = new UILabel(new Vector2(x, graphicsLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 17), color);

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
                (graphicsLabel, new SubstateOption<GraphicsMenu>()),
                (languageLabel, languageSetter)
            };
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
