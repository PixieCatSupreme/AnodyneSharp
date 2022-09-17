using AnodyneSharp.Dialogue;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources.Writing;
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

        private Dictionary<KeyFunctions, (int page, int loc)> _keysToLabel;

        (UILabel function, UILabel keyboard, UILabel controller, KeyFunctions keyFunction)? selectedKey;

        bool controllerMode;
        bool onConfirm;

        private Dictionary<KeyFunctions, InputChange> _changes;

        private UIEntity _bgBox;
        private UILabel pageLabel;
        private UILabel confirmLabel;
        private bool inConflict = false;

        private TextSelector _pageSetter;

        int _page;
        int _selectorPos;

        public ControlsSubstate()
        {
            _page = 0;

            _keyBindPages = new();

            _changes = new();

            SetLabels();

            _keysToLabel = _keyBindPages.SelectMany((page,pageNum) => page.Where((_,index) => index % 2 == 0).Select((e,index) => (e.keyFunction,(pageNum,index))))
                .ToDictionary((e) => e.keyFunction, (e) => e.Item2);

            _pageSetter = new TextSelector(new Vector2(91, 156), 32, 0, true, Drawing.DrawOrder.TEXT, "1/5", "2/5", "3/5", "4/5", "5/5")
            {
                noConfirm = true,
                noLoop = true
            };
        }

        private Keys? GetKey(KeyFunctions action, int index)
        {
            Keys? ret = null;
            if(_changes.TryGetValue(action,out InputChange val))
            {
                ret = val.GetKey(index);
            }
            if(!ret.HasValue)
            {
                var keys = KeyInput.RebindableKeys[action].Keys;
                if(keys.Count > index)
                    ret = KeyInput.RebindableKeys[action].Keys.ElementAtOrDefault(index);
            }
            return ret;
        }

        private void CheckConflicts()
        {
            IEnumerable<(KeyFunctions func,int index)> getIndex(int index) => _keysToLabel.Keys.Select(f => (f,index));

            var conflicts = getIndex(0).Concat(getIndex(1)).Select(loc => (loc, GetKey(loc.func, loc.index))).Where(l => l.Item2.HasValue)
                .GroupBy(l => l.Item2.Value).Where(g => g.Count() > 1).SelectMany(g => g.Select(e => e.loc));

            foreach(var p in _keyBindPages)
            {
                foreach(var s in p)
                {
                    s.keyboard.Color = Color.White;
                }
            }

            foreach(var (func, index) in conflicts)
            {
                var (page,loc) = _keysToLabel[func];
                _keyBindPages[page][loc * 2 + index].keyboard.Color = Color.Red;
            }

            if(conflicts.Any())
            {
                inConflict = true;
                confirmLabel.Color = Color.Red;
            }
            else
            {
                inConflict = false;
                confirmLabel.Color = Color.White;
            }
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
                if (_page == _keyBindPages.Count-1)
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
                    if (!inConflict)
                    {
                        SetKeyRebinds();

                        Exit = true;
                    }
                }
                else
                {
                    SelectKey();
                }

                SoundManager.PlaySoundEffect("menu_select");
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
                var keyValue = KeyInput.RebindableKeys[keyFunction];

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

            using InputConfigWriter inputConfigWriter = new($"{GameConstants.SavePath}InputConfig.dat");
            inputConfigWriter.WriteInputConfig();
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
            var keyFunc = selectedKey.Value.keyFunction;

            if (_changes.ContainsKey(keyFunc))
            {
                _changes[keyFunc] = change;
            }
            else
            {
                _changes.Add(keyFunc, change);
            }


            CheckConflicts();

            selectedKey = null;
            selector.Play("enabledRight");

            SoundManager.PlaySoundEffect("menu_select");
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
            pageLabel.SetText(DialogueManager.GetDialogue("misc", "any", "controls", 17 + _page));

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

            pageLabel = new UILabel(new Vector2(x + leftPadding, y + yStart), true, DialogueManager.GetDialogue("misc", "any", "controls", 17), layer: Drawing.DrawOrder.TEXT);
            confirmLabel = new UILabel(new Vector2(x + leftPadding, 156), true, DialogueManager.GetDialogue("misc", "any", "controls", 22), layer: Drawing.DrawOrder.TEXT);

            var keys = KeyInput.RebindableKeys;

            (UILabel function, UILabel keyboard, UILabel controller, KeyFunctions keyFunction) CreateTup(KeyFunctions key, int num, int pos) => (
                new UILabel(new Vector2(x + leftPadding, y + yStart + yStep * pos), true, (pos % 2 == 1) ? DialogueManager.GetDialogue("misc", "any", "controls", 1 + num) : "",
                    layer: Drawing.DrawOrder.TEXT),
                new UILabel(new Vector2(x + leftPadding + controlsOffset, y + yStart + yStep * pos), true,
                        (keys[key].Keys.Count > ((pos-1) % 2) ? GetKeyBoardString(keys[key].Keys[((pos - 1) % 2)]) : ""),
                    layer: Drawing.DrawOrder.TEXT),
                new UILabel(new Vector2(x + leftPadding + controlsOffset + buttonSpacing, y + yStart + yStep * pos), true,
                    (keys[key].Buttons.Count > ((pos-1)%2) ? GetButtonString(keys[key].Buttons[((pos - 1) % 2)]) : ""),
                    layer: Drawing.DrawOrder.TEXT),
                key);

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.Up, 1, 1),
                CreateTup(KeyFunctions.Up, 1, 2),
                CreateTup(KeyFunctions.Right, 4, 3),
                CreateTup(KeyFunctions.Right, 4, 4),
                CreateTup(KeyFunctions.Down, 2, 5),
                CreateTup(KeyFunctions.Down, 2, 6),
                CreateTup(KeyFunctions.Left, 3, 7),
                CreateTup(KeyFunctions.Left, 3, 8),
            });

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.Cancel, 5, 1),
                CreateTup(KeyFunctions.Cancel, 5, 2),
                CreateTup(KeyFunctions.Accept, 6, 3),
                CreateTup(KeyFunctions.Accept, 6, 4),
                CreateTup(KeyFunctions.Pause, 7, 5),
                CreateTup(KeyFunctions.Pause, 7, 6),
            });

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.Broom1, 8, 1),
                CreateTup(KeyFunctions.Broom1, 8, 2),
                CreateTup(KeyFunctions.Broom2, 9, 3),
                CreateTup(KeyFunctions.Broom2, 9, 4),
                CreateTup(KeyFunctions.Broom3, 10, 5),
                CreateTup(KeyFunctions.Broom3, 10, 6),
                CreateTup(KeyFunctions.Broom4, 11, 7),
                CreateTup(KeyFunctions.Broom4, 11, 8),
            });

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.NextPage, 12, 1),
                CreateTup(KeyFunctions.NextPage, 12, 2),
                CreateTup(KeyFunctions.PreviousPage, 13, 3),
                CreateTup(KeyFunctions.PreviousPage, 13, 4),
            });

            _keyBindPages.Add(new()
            {
                CreateTup(KeyFunctions.QuickSave, 14, 1),
                CreateTup(KeyFunctions.QuickSave, 14, 2),
                CreateTup(KeyFunctions.QuickLoad, 15, 3),
                CreateTup(KeyFunctions.QuickLoad, 15, 4),
            });
        }

        private struct InputChange
        {
            public Keys? key1;
            public Keys? key2;

            public Keys? GetKey(int index)
            {
                if (index == 0) return key1;
                return key2;
            }

            public Buttons? button1;
            public Buttons? button2;

            public Buttons? GetButton(int index)
            {
                if (index == 0) return button1;
                return button2;
            }
        }
    }
}
