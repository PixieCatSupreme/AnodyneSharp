using AnodyneSharp.Dialogue;
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
using System.Linq;
using System.Text;

using static AnodyneSharp.Utilities.TextUtilities;

namespace AnodyneSharp.States.MenuSubstates
{
    public class ControlsSubstate : Substate
    {
        private List<List<(UILabel function, UILabel keyboard, UILabel controller, KeyFunctions keyFunction)>> _keyBindPages;

        (UILabel function, UILabel keyboard, UILabel controller, KeyFunctions keyFunction)? selectedKey;

        bool controllerMode;
        bool onConfirm;

        private Dictionary<KeyFunctions, InputChange> _changes;

        private UIEntity _bgBox;
        private UILabel pageLabel;
        private UILabel confirmLabel;

        private TextSelector _pageSetter;



        int _page;
        int _selectorPos;

        public ControlsSubstate()
        {
            _page = 0;

            _keyBindPages = new();

            _changes = new();

            SetLabels();

            _pageSetter = new TextSelector(new Vector2(91, 156), 32, 0, true, Drawing.DrawOrder.TEXT, "1/4", "2/4", "3/4", "4/4")
            {
                noConfirm = true,
                noLoop = true
            };
        }

        public override void GetControl()
        {
            base.GetControl();

            _page = 0;

            SetCursorPos(0);
        }


        public override void DrawUI()
        {
            base.DrawUI();

            _pageSetter.Draw();

            pageLabel.Draw();

            confirmLabel.Draw();

            foreach (var (function, keyboard, controller, _) in _keyBindPages[_page])
            {
                function.Draw();
                keyboard.Draw();
                controller.Draw();
            }

            _bgBox.Draw();
        }

        public override void HandleInput()
        {
            if (selectedKey == null)
            {
                SelectionInput();
            }
            else
            {
                if (controllerMode)
                {
                    if (KeyInput.IsAnyButtonPressed(out Buttons? b))
                    {
                        ButtonRebind(b.Value);
                    }
                }
                else if (KeyInput.IsAnyKeyPressed(out Keys? key))
                {
                    KeyRebind(key.Value);
                }
            }
        }

        private void SelectionInput()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
            {
                ExitSubState();
                return;
            }

            bool moved = false;

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                SetCursorPos(_selectorPos - 1);
                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {
                SetCursorPos(_selectorPos + 1);
                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
            {
                if (_page == 3)
                {
                    return;
                }

                moved = true;

                _page++;

                _pageSetter.SetValue(_page);
                PageValueChanged();
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
            {
                if (_page == 0)
                {
                    return;
                }

                moved = true;

                _page--;

                _pageSetter.SetValue(_page);
                PageValueChanged();
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                if (onConfirm)
                {
                    SetKeyRebinds();

                    Exit = true;
                }
                else
                {
                    SelectKey();
                }
            }


            if (moved)
            {
                SoundManager.PlaySoundEffect("menu_move");
            }
        }

        private void SetKeyRebinds()
        {
            foreach (var (keyFunction, change) in _changes)
            {
                var keyValue = KeyInput.RebindableKeys.FirstOrDefault(k => k.Key == keyFunction).Value;

                if (controllerMode)
                {
                    if (change.button1 != null)
                    {
                        if (keyValue.Buttons.Count == 0)
                        {
                            keyValue.Buttons.Add(change.button1.Value);
                        }
                        else
                        {
                            keyValue.Buttons[0] = change.button1.Value;
                        }
                    }
                    else if (change.button2 != null)
                    {
                        if (keyValue.Buttons.Count < 1)
                        {
                            keyValue.Buttons.Add(change.button2.Value);
                        }
                        else
                        {
                            keyValue.Buttons[1] = change.button2.Value;
                        }
                    }
                }
                else
                {
                    if (change.key1 != null)
                    {
                        if (keyValue.Keys.Count == 0)
                        {
                            keyValue.Keys.Add(change.key1.Value);
                        }
                        else
                        {
                            keyValue.Keys[0] = change.key1.Value;
                        }
                    }
                    else if (change.key2 != null)
                    {
                        if (keyValue.Keys.Count < 1)
                        {
                            keyValue.Keys.Add(change.key2.Value);
                        }
                        else
                        {
                            keyValue.Keys[1] = change.key2.Value;
                        }
                    }
                }
            }
        }

        private void KeyRebind(Keys key)
        {
            InputChange change = new();

            //Check of first line is selected
            if (_selectorPos % 2 == 0)
            {
                change.key1 = key;
            }
            else
            {
                change.key2 = key;
            }

            _keyBindPages[_page][_selectorPos].keyboard.SetText(GetKeyBoardString(key));

            ExitRebind(change);
        }

        private void ButtonRebind(Buttons button)
        {
            InputChange change = new();

            //Check of first line is selected
            if (_selectorPos % 2 == 0)
            {
                change.button1 = button;
            }
            else
            {
                change.button2 = button;
            }

            _keyBindPages[_page][_selectorPos].controller.SetText(GetButtonString(button));

            ExitRebind(change);
        }

        private void ExitRebind(InputChange change)
        {
            _changes.Add(selectedKey.Value.keyFunction, change);

            selectedKey = null;
            selector.Play("enabledRight");
        }

        private void SelectKey()
        {
            selectedKey = _keyBindPages[_page][_selectorPos];

            if (!selectedKey.HasValue)
            {
                return;
            }

            selector.Play("disabledRight");

            controllerMode = KeyInput.ControllerMode;

            if (controllerMode)
            {
                selectedKey.Value.controller.SetText("_");
            }
            else
            {
                selectedKey.Value.keyboard.SetText("_");
            }
        }

        private void SetCursorPos(int newSelectorPos)
        {
            if (newSelectorPos < 0 || newSelectorPos > _keyBindPages[_page].Count)
            {
                return;
            }

            _selectorPos = newSelectorPos;

            if (newSelectorPos > _keyBindPages[_page].Count - 1)
            {
                selector.Position = confirmLabel.Position - new Vector2(selector.sprite.Width, -2);
                onConfirm = true;
            }
            else
            {
                selector.Position = _keyBindPages[_page][_selectorPos].function.Position - new Vector2(selector.sprite.Width, -2);
                onConfirm = false;
            }

            selector.Position.Y += CursorOffset;
        }

        private void PageValueChanged()
        {
            pageLabel.SetText(DialogueManager.GetDialogue("misc", "any", "controls", 15 + _page));

            if (onConfirm)
            {
                SetCursorPos(_keyBindPages[_page].Count);
            }
            else if (_selectorPos > _keyBindPages[_page].Count - 1)
            {
                SetCursorPos(_keyBindPages[_page].Count - 1);
            }
        }

        private void SetLabels()
        {
            int leftPadding = 10;
            int menuWidth = 140;
            int controlsOffset = 70;
            int buttonSpacing = 44;

            if (GlobalState.CurrentLanguage == Language.ZH_CN)
            {
                controlsOffset -= 20;
                buttonSpacing += 20;
            }

            float x = GameConstants.SCREEN_WIDTH_IN_PIXELS / 2 - menuWidth / 2;
            float y = 8;
            float yStep = GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 8;

            float yStart = (GameConstants.FONT_LINE_HEIGHT - GameConstants.LineOffset + 8) / 2;

            _bgBox = new UIEntity(new Vector2(x, y), "controls", menuWidth, 160, Drawing.DrawOrder.TEXTBOX);

            pageLabel = new UILabel(new Vector2(x + leftPadding, y + yStart), true, DialogueManager.GetDialogue("misc", "any", "controls", 15), layer: Drawing.DrawOrder.TEXT);
            confirmLabel = new UILabel(new Vector2(x + leftPadding, 156), true, DialogueManager.GetDialogue("misc", "any", "controls", 19), layer: Drawing.DrawOrder.TEXT);

            var keys = KeyInput.RebindableKeys;

            (UILabel function, UILabel keyboard, UILabel controller, KeyFunctions keyFunction) CreateTup(KeyFunctions key, bool isSecond, int num, int pos) => (
                new UILabel(new Vector2(x + leftPadding, y + yStart + yStep * pos), true, !isSecond ? DialogueManager.GetDialogue("misc", "any", "controls", 1 + num) : "",
                    layer: Drawing.DrawOrder.TEXT),
                new UILabel(new Vector2(x + leftPadding + controlsOffset, y + yStart + yStep * pos), true,
                    !isSecond
                        ? (keys[key].Keys.Any() ? GetKeyBoardString(keys[key].Keys.FirstOrDefault()) : "")
                        : (keys[key].Keys.Count > 1 ? GetKeyBoardString(keys[key].Keys.ElementAtOrDefault(1)) : ""),
                    layer: Drawing.DrawOrder.TEXT),
                new UILabel(new Vector2(x + leftPadding + controlsOffset + buttonSpacing, y + yStart + yStep * pos), true,
                    !isSecond
                        ? (keys[key].Buttons.Any() ? GetButtonString(keys[key].Buttons.FirstOrDefault()) : "")
                        : (keys[key].Buttons.Count > 1 ? GetButtonString(keys[key].Buttons.ElementAtOrDefault(1)) : ""),
                    layer: Drawing.DrawOrder.TEXT),
                key);

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.Up, false, 1, 1),
                CreateTup(KeyFunctions.Up, true, 1, 2),
                CreateTup(KeyFunctions.Right, false, 4, 3),
                CreateTup(KeyFunctions.Right, true, 4, 4),
                CreateTup(KeyFunctions.Down, false, 2, 5),
                CreateTup(KeyFunctions.Down, true, 2, 6),
                CreateTup(KeyFunctions.Left, false, 3, 7),
                CreateTup(KeyFunctions.Left, true, 3, 8),
            });

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.Cancel, false, 5, 1),
                CreateTup(KeyFunctions.Cancel, true, 5, 2),
                CreateTup(KeyFunctions.Accept, false, 6, 3),
                CreateTup(KeyFunctions.Accept, true, 6, 4),
                CreateTup(KeyFunctions.Pause, false, 7, 5),
                CreateTup(KeyFunctions.Pause, true, 7, 6),
            });

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.Broom1, false, 8, 1),
                CreateTup(KeyFunctions.Broom1, true, 8, 2),
                CreateTup(KeyFunctions.Broom2, false, 9, 3),
                CreateTup(KeyFunctions.Broom2, true, 9, 4),
                CreateTup(KeyFunctions.Broom3, false, 10, 5),
                CreateTup(KeyFunctions.Broom3, true, 10, 6),
                CreateTup(KeyFunctions.Broom4, false, 11, 7),
                CreateTup(KeyFunctions.Broom4, true, 11, 8),
            });

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.NextPage, false, 12, 1),
                CreateTup(KeyFunctions.NextPage, true, 12, 2),
                CreateTup(KeyFunctions.PreviousPage, false, 13, 3),
                CreateTup(KeyFunctions.PreviousPage, true, 13, 4),
            });
        }

        private struct InputChange
        {
            public Keys? key1;
            public Keys? key2;

            public Buttons? button1;
            public Buttons? button2;
        }
    }
}
