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
    public class GraphicsMenu : ListSubstate
    {
        UIEntity _bgBox;
        UILabel _flashLabel;

        public GraphicsMenu()
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
            _flashLabel.Draw();
        }

        protected override void SetLabels()
        {
            float x = GameConstants.SCREEN_WIDTH_IN_PIXELS / 2 - 136 / 2;
            float menuX = x + 10;
            float y = 10;
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 3;

            _bgBox = new UIEntity(new Vector2(x, y), "pop_menu", 136, 126, Drawing.DrawOrder.TEXTBOX);

            var resolutionLabel = new UILabel(new Vector2(menuX, y + yStep), true, DialogueManager.GetDialogue("misc", "any", "config", 12), layer: Drawing.DrawOrder.TEXT);
            var scalingLabel = new UILabel(new Vector2(menuX, resolutionLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 16), layer: Drawing.DrawOrder.TEXT);

            var resolutionSelect = new TextSelector(resolutionLabel.Position + new Vector2(10, yStep), 90, (int)GlobalState.settings.resolution, false, Drawing.DrawOrder.TEXT,
                DialogueManager.GetDialogue("misc", "any", "config", 13), DialogueManager.GetDialogue("misc", "any", "config", 14), DialogueManager.GetDialogue("misc", "any", "config", 15))
            {
                ValueChangedEvent = (s, index) => { GlobalState.ResolutionDirty = GlobalState.settings.resolution != (Resolution)index; GlobalState.settings.resolution = (Resolution)index; }
            };

            var scalingSelect = new TextSelector(scalingLabel.Position + new Vector2(50, 0), 20, GlobalState.settings.scale - 1, true, Drawing.DrawOrder.TEXT, "1x", "2x", "3x", "4x")
            {
                ValueChangedEvent = (s, index) => { GlobalState.ResolutionDirty = GlobalState.settings.scale != index + 1; GlobalState.settings.scale = index + 1; },
                noLoop = true,
                noConfirm = true
            };

            _flashLabel = new UILabel(new Vector2(menuX, scalingLabel.Position.Y + yStep * 1.5f), true, "Screen flash", layer: Drawing.DrawOrder.TEXT, forceEnglish: true);

            var brightnessLabel = new UILabel(new Vector2(menuX + 8, _flashLabel.Position.Y + yStep), true, "Max:", layer: Drawing.DrawOrder.TEXT, forceEnglish: true);

            var brightnessSelect = new AudioSlider(brightnessLabel.Position + Vector2.UnitX * 40, GlobalState.settings.flash_brightness, 0f, 1f, 0.1f, false, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (value, index) => { GlobalState.settings.flash_brightness = value; }
            };

            var easingLabel = new UILabel(brightnessLabel.Position + Vector2.UnitY * yStep, true, "Easing:", layer: Drawing.DrawOrder.TEXT, forceEnglish: true);

            var easingSelect = new AudioSlider(easingLabel.Position + Vector2.UnitX * 40, GlobalState.settings.flash_easing, 0f, 0.2f, 0.02f, false, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (value, index) => { GlobalState.settings.flash_easing = value; }
            };

            var testLabel = new UILabel(easingLabel.Position + Vector2.UnitY * yStep, true, "Test", layer: Drawing.DrawOrder.TEXT, forceEnglish: true);

            var shakeLabel = new UILabel(new Vector2(menuX, testLabel.Position.Y + yStep * 1.5f), true, "Screen shake:", layer: Drawing.DrawOrder.TEXT, forceEnglish: true);

            var shakeSetting = new CheckBox(new Vector2(x + 120, shakeLabel.Position.Y), GlobalState.settings.screenshake, false, Drawing.DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.screenshake = val; }
            };

            options = new()
            {
                (resolutionLabel, resolutionSelect),
                (scalingLabel, scalingSelect),
                (brightnessLabel, brightnessSelect),
                (easingLabel, easingSelect),
                (testLabel, new ActionOption(() => GlobalState.flash.Flash(1.0f, Color.White))),
                (shakeLabel, shakeSetting)
            };
        }
    }
}
