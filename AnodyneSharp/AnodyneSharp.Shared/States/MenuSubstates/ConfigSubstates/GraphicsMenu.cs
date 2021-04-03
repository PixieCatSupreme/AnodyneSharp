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
        }

        protected override void SetLabels()
        {
            float x = GameConstants.SCREEN_WIDTH_IN_PIXELS / 2 - 136 / 2;
            float y = 10;
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 8;

            _bgBox = new UIEntity(new Vector2(x, y), "controls", 136, 126, Drawing.DrawOrder.TEXTBOX);

            var resolutionLabel = new UILabel(new Vector2(x + 10, y + yStep * 2), true, DialogueManager.GetDialogue("misc", "any", "config", 12), layer: Drawing.DrawOrder.TEXT);
            var scalingLabel = new UILabel(new Vector2(x + 10, resolutionLabel.Position.Y + yStep * 3), true, DialogueManager.GetDialogue("misc", "any", "config", 16), layer: Drawing.DrawOrder.TEXT);

            var resolutionSelect = new TextSelector(resolutionLabel.Position + new Vector2(10, yStep), 90,(int)GlobalState.settings.resolution, false, Drawing.DrawOrder.TEXT,
                DialogueManager.GetDialogue("misc", "any", "config", 13), DialogueManager.GetDialogue("misc", "any", "config", 14), DialogueManager.GetDialogue("misc", "any", "config", 15))
            {
                ValueChangedEvent = (s,index) => { GlobalState.ResolutionDirty = GlobalState.settings.resolution != (Resolution)index; GlobalState.settings.resolution = (Resolution)index; }
            };

            var scalingSelect = new TextSelector(scalingLabel.Position + new Vector2(50, 0), 20, GlobalState.settings.scale - 1, true, Drawing.DrawOrder.TEXT, "1x", "2x", "3x", "4x")
            {
                ValueChangedEvent = (s, index) => { GlobalState.ResolutionDirty = GlobalState.settings.scale != index + 1; GlobalState.settings.scale = index + 1; },
                noLoop = true,
                noConfirm = true
            };

            options = new()
            {
                (resolutionLabel,resolutionSelect),
                (scalingLabel,scalingSelect)
            };
        }
    }
}
