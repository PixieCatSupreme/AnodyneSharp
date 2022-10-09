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
            float x = 60 + (_isInMainMenu ? 0 : 9);
            float y = 28 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset;
            float sliderOffset = GlobalState.CurrentLanguage == Language.ZH_CN ? 2 : 0;

            Color color = _isInMainMenu ? new Color(116, 140, 144) : Color.White;

            var keybindsLabel = new UILabel(new Vector2(x, y), true, DialogueManager.GetDialogue("misc", "any", "config", 1), color);

            var bgmLabel = new UILabel(new Vector2(x, keybindsLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 23), color);
            var sfxLabel = new UILabel(new Vector2(x, bgmLabel.Position.Y + 12), true, DialogueManager.GetDialogue("misc", "any", "config", 24), color);

            var gameplayLabel = new UILabel(new Vector2(x, sfxLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 25), color);

            var graphicsLabel = new UILabel(new Vector2(x, gameplayLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 26), color);
            
            var languageLabel = new UILabel(new Vector2(x, graphicsLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 17), color);

            var musicSlider = new AudioSlider(new Vector2(bgmLabel.Position.X + bgmLabel.Writer.WriteArea.Width - 5, bgmLabel.Position.Y + sliderOffset), GlobalState.settings.music_volume_scale, 0f, 1f, 0.1f, _isInMainMenu ? MenuStyle.MainMenu : MenuStyle.PauseMenu)
            {
                ValueChangedEvent = (value,index) => { float vol = SoundManager.GetVolume(); GlobalState.settings.music_volume_scale = value; SoundManager.SetSongVolume(vol); }
            };

            var sfxSlider = new AudioSlider(new Vector2(sfxLabel.Position.X + sfxLabel.Writer.WriteArea.Width - 5, sfxLabel.Position.Y + sliderOffset), GlobalState.settings.sfx_volume_scale, 0f, 1f, 0.1f, _isInMainMenu ? MenuStyle.MainMenu : MenuStyle.PauseMenu)
            {
                ValueChangedEvent = (value,index) => { GlobalState.settings.sfx_volume_scale = value; }
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
                (gameplayLabel, new SubstateOption<GameplayMenu>()),
                (graphicsLabel, new SubstateOption<GraphicsMenu>()),
                (languageLabel, languageSetter)
            };
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
