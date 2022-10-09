using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu.Config;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
            bool isChinese = GlobalState.CurrentLanguage == Language.ZH_CN;

            float x = GameConstants.SCREEN_WIDTH_IN_PIXELS / 2 - 136 / 2;
            float menuX = x + 10;
            float y = -4;
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 3 + (isChinese ? 2 : 0);

            float sliderOffset = GlobalState.CurrentLanguage == Language.ZH_CN ? 4 : 0;

            _bgBox = new UIEntity(new Vector2(x, 5), "pop_menu", 136, 126, DrawOrder.TEXTBOX);

            var resolutionLabel = new UILabel(new Vector2(menuX, y + yStep), true, DialogueManager.GetDialogue("misc", "any", "config", 12), layer: DrawOrder.TEXT);
            var scalingLabel = new UILabel(new Vector2(menuX, resolutionLabel.Position.Y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 16), layer: DrawOrder.TEXT);

            var resolutionSelect = new TextSelector(resolutionLabel.Position + new Vector2(10, yStep), 90, (int)GlobalState.settings.resolution, false, DrawOrder.TEXT,
                DialogueManager.GetDialogue("misc", "any", "config", 13), DialogueManager.GetDialogue("misc", "any", "config", 14), DialogueManager.GetDialogue("misc", "any", "config", 15))
            {
                ValueChangedEvent = (s, index) => { GlobalState.ResolutionDirty = GlobalState.settings.resolution != (Resolution)index; GlobalState.settings.resolution = (Resolution)index; }
            };

            var scalingSelect = new TextSelector(scalingLabel.Position + new Vector2(50, sliderOffset), 20, GlobalState.settings.scale - 1, true, DrawOrder.TEXT, Enumerable.Range(1, SpriteDrawer.MaxScale).Select(n => $"{n}x").ToArray())
            {
                ValueChangedEvent = (s, index) => { GlobalState.ResolutionDirty = GlobalState.settings.scale != index + 1; GlobalState.settings.scale = index + 1; },
                noLoop = true,
                noConfirm = true
            };

            var fpsLabel = new UILabel(scalingLabel.Position + Vector2.UnitY * yStep, true, DialogueManager.GetDialogue("misc", "any", "graphics", 0), layer: DrawOrder.TEXT, forceEnglish: true);
            var fpsSelect = new TextSelector(fpsLabel.Position + Vector2.UnitX * 30, 80, (int)GlobalState.settings.fps, true, DrawOrder.TEXT, DialogueManager.GetDialogue("misc", "any", "graphics", 1), DialogueManager.GetDialogue("misc", "any", "graphics", 2), DialogueManager.GetDialogue("misc", "any", "graphics", 3))
            {
                ValueChangedEvent = (s, index) => { GlobalState.ResolutionDirty = GlobalState.settings.fps != (FPS)index; GlobalState.settings.fps = (FPS)index; }
            };

            _flashLabel = new UILabel(new Vector2(menuX, fpsLabel.Position.Y + (int)(yStep * (isChinese ? 1f : 1.5f))), true, DialogueManager.GetDialogue("misc", "any", "graphics", 4), layer: DrawOrder.TEXT);

            var brightnessLabel = new UILabel(new Vector2(menuX + 8, _flashLabel.Position.Y + yStep), true, DialogueManager.GetDialogue("misc", "any", "graphics", 5), layer: DrawOrder.TEXT, forceEnglish: isChinese);

            var brightnessSelect = new AudioSlider(brightnessLabel.Position + Vector2.UnitX * 40, GlobalState.settings.flash_brightness, 0f, 1f, 0.1f,  MenuStyle.SubMenu, DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (value, index) => { GlobalState.settings.flash_brightness = value; }
            };

            var easingLabel = new UILabel(brightnessLabel.Position + Vector2.UnitY * yStep, true, DialogueManager.GetDialogue("misc", "any", "graphics", 6), layer: DrawOrder.TEXT, forceEnglish: isChinese);

            var easingSelect = new AudioSlider(easingLabel.Position + Vector2.UnitX * 40, GlobalState.settings.flash_easing, 0f, 0.2f, 0.02f, MenuStyle.SubMenu, DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (value, index) => { GlobalState.settings.flash_easing = value; }
            };

            var testLabel = new UILabel(easingLabel.Position + Vector2.UnitY * yStep, true, DialogueManager.GetDialogue("misc", "any", "graphics", 7), layer: DrawOrder.TEXT);

            var shakeLabel = new UILabel(new Vector2(menuX, testLabel.Position.Y + (int)(yStep * (isChinese ? 1.2f : 1.5f))), true, DialogueManager.GetDialogue("misc", "any", "graphics", 8), layer: DrawOrder.TEXT, forceEnglish: isChinese);

            var shakeSetting = new CheckBox(new Vector2(x + 120, shakeLabel.Position.Y), GlobalState.settings.screenshake, MenuStyle.SubMenu, DrawOrder.SUBMENU_SLIDER)
            {
                ValueChangedEvent = (val) => { GlobalState.settings.screenshake = val; }
            };

            options = new()
            {
                (resolutionLabel, resolutionSelect),
                (scalingLabel, scalingSelect),
                (fpsLabel, fpsSelect),
                (brightnessLabel, brightnessSelect),
                (easingLabel, easingSelect),
                (testLabel, new ActionOption(() => GlobalState.flash.Flash(1.0f, Color.White))),
                (shakeLabel, shakeSetting)
            };
        }
    }
}
