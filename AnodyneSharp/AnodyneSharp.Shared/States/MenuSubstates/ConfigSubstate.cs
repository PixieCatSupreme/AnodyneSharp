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
            float x = 60 + (_isInMainMenu ? 6 : 9);
            float y = 28 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0);
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset;

            Color color = _isInMainMenu ? new Color(116, 140, 144) : Color.White;

            var keybindsLabel = new UILabel(new Vector2(x, y), true, DialogueManager.GetDialogue("misc", "any", "config", 1), color);

            var bgmLabel = new UILabel(new Vector2(x, keybindsLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 23), color, forceEnglish:true);
            var sfxLabel = new UILabel(new Vector2(x, bgmLabel.Position.Y + 12), true, DialogueManager.GetDialogue("misc", "any", "config", 24), color, forceEnglish:true);

            var gameplayLabel = new UILabel(new Vector2(x, sfxLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 25), color);

            var graphicsLabel = new UILabel(new Vector2(x, gameplayLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 26), color);
            
            var languageLabel = new UILabel(new Vector2(x, graphicsLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 17), color);

            var musicSlider = new AudioSlider(new Vector2(bgmLabel.Position.X + bgmLabel.Writer.WriteArea.Width - 5, bgmLabel.Position.Y), GlobalState.settings.music_volume_scale, 0f, 1f, 0.1f, _isInMainMenu ? MenuStyle.MainMenu : MenuStyle.PauseMenu)
            {
                ValueChangedEvent = (value,index) => { float vol = SoundManager.GetVolume(); GlobalState.settings.music_volume_scale = value; SoundManager.SetSongVolume(vol); }
            };

            var sfxSlider = new AudioSlider(new Vector2(sfxLabel.Position.X + sfxLabel.Writer.WriteArea.Width - 5, sfxLabel.Position.Y), GlobalState.settings.sfx_volume_scale, 0f, 1f, 0.1f, _isInMainMenu ? MenuStyle.MainMenu : MenuStyle.PauseMenu)
            {
                ValueChangedEvent = (value,index) => { GlobalState.settings.sfx_volume_scale = value; }
            };

            static string ToString(Language l)
            {
                return Enum.GetName(typeof(Language), l).ToLower().Replace('_','-');
            }

            string[] languageNames = Enum.GetValues<Language>().Where(l=>l == GlobalState.CurrentLanguage || (l != Language.JP && l!= Language.KR)).Select(l=>ToString(l)).ToArray();

            var languageSetter = new TextSelector(new Vector2(x + languageLabel.Writer.WriteArea.Width - 8, languageLabel.Position.Y + GameConstants.LineOffset), GlobalState.CurrentLanguage == Language.ZH_CN ? 40 : 30, Array.FindIndex(languageNames,s=>s == ToString(GlobalState.CurrentLanguage)), true, languageNames)
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
            Language lang = Enum.Parse<Language>(value.Replace('-', '_').ToUpper());
            GlobalState.settings.language = lang;
            DialogueManager.Reload();

            SetLabels();
            GlobalState.RefreshLabels = true;
        }
    }
}
