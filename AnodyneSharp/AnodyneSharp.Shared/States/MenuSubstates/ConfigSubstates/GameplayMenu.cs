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

            _bgBox = new UIEntity(new Vector2(x, y), "pop_menu", 136, 126, Drawing.DrawOrder.TEXTBOX);

            var autosaveLabel = new UILabel(new Vector2(menuX, y + yStep * 1.5f), true, DialogueManager.GetDialogue("misc", "any", "config", 3), layer: Drawing.DrawOrder.TEXT);

            var autosaveSetting = new CheckBox(new Vector2(x + 120, autosaveLabel.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 5 : 2)), GlobalState.settings.autosave_on,  MenuStyle.SubMenu, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.autosave_on = val; }
            };

            var focusLabel = new UILabel(autosaveLabel.Position + Vector2.UnitY * yStep *1.8f, true, "Unfocus pause:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var focusSetting = new CheckBox(new Vector2(x + 120, focusLabel.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.pause_on_unfocus, MenuStyle.SubMenu, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.pause_on_unfocus = val; }
            };


            var fastText = new UILabel(focusLabel.Position + Vector2.UnitY * yStep *1.5f, true, "Fast text:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var fastTextSetting = new CheckBox(new Vector2(x + 120, fastText.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.fast_text, MenuStyle.SubMenu, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.fast_text = val; }
            };

            var invincibility = new UILabel(fastText.Position + Vector2.UnitY * yStep, true, "Invincibility:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var invincibilitySetting = new CheckBox(new Vector2(x + 120, invincibility.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.invincible, MenuStyle.SubMenu, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.invincible = val; }
            };

            var healthdrop = new UILabel(invincibility.Position + Vector2.UnitY * yStep, true, "Health drops+:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var healthdropSetting = new CheckBox(new Vector2(x + 120, healthdrop.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.guaranteed_health, MenuStyle.SubMenu, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.guaranteed_health = val; }
            };

            var coyote = new UILabel(healthdrop.Position + Vector2.UnitY * yStep, true, "Coyote time+:", forceEnglish: true, layer: Drawing.DrawOrder.TEXT);

            var coyoteSetting = new CheckBox(new Vector2(x + 120, coyote.Position.Y + (GlobalState.CurrentLanguage == Language.ZH_CN ? 3 : 0)), GlobalState.settings.extended_coyote, MenuStyle.SubMenu, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.extended_coyote = val; }
            };

            options = new()
            {
                (autosaveLabel, autosaveSetting),
                (focusLabel, focusSetting),
                (fastText, fastTextSetting),
                (invincibility, invincibilitySetting),
                (healthdrop,healthdropSetting),
                (coyote, coyoteSetting)
            };
        }
    }
}
