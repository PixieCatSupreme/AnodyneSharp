using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu.Config;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.MenuSubstates.ConfigSubstates
{
    public class GameplayMenu : ListSubstate
    {
        UIEntity _bgBox;

        public GameplayMenu()
        {
            SetLabels();
        }

        protected override void OnExit()
        {
            base.OnExit();
            GlobalState.settings.Save();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            _bgBox.Draw();
        }

        protected override void SetLabels()
        {
            float x = GameConstants.SCREEN_WIDTH_IN_PIXELS / 2 - 136 / 2;
            float menuX = x + 10;
            float y = 10;
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 5;

            _bgBox = new UIEntity(new Vector2(x, y), "controls", 136, 126, Drawing.DrawOrder.TEXTBOX);

            var autosaveLabel = new UILabel(new Vector2(menuX, y + yStep * 1.5f), true, DialogueManager.GetDialogue("misc", "any", "config", 3), layer: Drawing.DrawOrder.TEXT);

            var autosaveSetting = new CheckBox(new Vector2(x + 120, autosaveLabel.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 5 : 2)), GlobalState.settings.autosave_on, false, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.autosave_on = val; }
            };

            var fastText = new UILabel(autosaveLabel.Position + Vector2.UnitY * yStep * 2f, true, "Fast text:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var fastTextSetting = new CheckBox(new Vector2(x + 120, fastText.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.fast_text, false, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.fast_text = val; }
            };

            var invincibility = new UILabel(fastText.Position + Vector2.UnitY * yStep, true, "Invincibility:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var invincibilitySetting = new CheckBox(new Vector2(x + 120, invincibility.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.invincible, false, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.invincible = val; }
            };

            var healthdrop = new UILabel(invincibility.Position + Vector2.UnitY * yStep, true, "Health drops+:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var healthdropSetting = new CheckBox(new Vector2(x + 120, healthdrop.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.guaranteed_health, false, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.guaranteed_health = val; }
            };

            var coyote = new UILabel(healthdrop.Position + Vector2.UnitY * yStep, true, "Coyote time+:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var coyoteSetting = new CheckBox(new Vector2(x + 120, coyote.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.extended_coyote, false, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.extended_coyote = val; }
            };

            options = new()
            {
                (autosaveLabel, autosaveSetting),
                (fastText, fastTextSetting),
                (invincibility, invincibilitySetting),
                (healthdrop,healthdropSetting),
                (coyote, coyoteSetting)
            };
        }
    }
}
