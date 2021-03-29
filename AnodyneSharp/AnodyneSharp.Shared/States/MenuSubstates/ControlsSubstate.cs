using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static AnodyneSharp.Utilities.TextUtilities;

namespace AnodyneSharp.States.MenuSubstates
{


    public class ControlsSubstate : Substate
    {
        List<(UILabel function, UILabel keyboard, UILabel controller, KeyFunctions keyFunction)> keyBinds;

        UIEntity _bgBox;

        public ControlsSubstate()
        {
            SetLabels();
        }

        public override void DrawUI()
        {
            foreach (var l in keyBinds)
            {
                l.function.Draw();
                l.keyboard.Draw();
                l.controller.Draw();
            }

            _bgBox.Draw();
        }

        public override void HandleInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel) || KeyInput.JustPressedRebindableKey(KeyFunctions.Pause))
            {
                ExitSubState();
            }
        }

        private void SetLabels()
        {
            float x = GameConstants.SCREEN_WIDTH_IN_PIXELS / 2 - 136 / 2;
            float y = 10;
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset +8;

            _bgBox = new UIEntity(new Vector2(x, y), "controls", 136, 126, Drawing.DrawOrder.TEXTBOX);

            var keys = KeyInput.RebindableKeys;

            Func<KeyFunctions, int, (UILabel function, UILabel keyboard, UILabel controller, KeyFunctions keyFunction)> CreateTup =
                (key, num) => (
                new UILabel(new Vector2(x + 10, y + yStep * num), true, DialogueManager.GetDialogue("misc", "any", "controls", 1 + num), layer: Drawing.DrawOrder.TEXT),
                new UILabel(new Vector2(x + 70, y + yStep * num), true, keys[key].Keys.Any() ? GetKeyBoardString(keys[key].Keys.FirstOrDefault()) : "", layer: Drawing.DrawOrder.TEXT),
                new UILabel(new Vector2(x + 110, y + yStep * num), true, keys[key].Buttons.Any() ?  GetButtonString(keys[key].Buttons.FirstOrDefault()) : "", layer: Drawing.DrawOrder.TEXT),
                key);

            keyBinds = new ()
            {
                CreateTup(KeyFunctions.Up, 1),
                CreateTup(KeyFunctions.Down, 2),
                CreateTup(KeyFunctions.Left, 3),
                CreateTup(KeyFunctions.Right, 4),
                CreateTup(KeyFunctions.Accept, 5),
                CreateTup(KeyFunctions.Cancel, 6),
                CreateTup(KeyFunctions.Pause, 7),
                //CreateTup(KeyFunctions.Broom1, 9),
                //CreateTup(KeyFunctions.Broom2, 10),
                //CreateTup(KeyFunctions.Broom3, 11),
                //CreateTup(KeyFunctions.Broom4, 12),
                //CreateTup(KeyFunctions.NextPage, 13),
                //CreateTup(KeyFunctions.PreviousPage, 14),
            };
        }
    }
}
