using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.PauseSubstates.BaseSubstates
{
    public class GridSubstate : DialogueSubstate
    {
        private string _dialogueName;
        private string _textureName;

        private int _selectedID;
        private UIEntity[] _items;
        private bool[] _collectedStats;

        public GridSubstate(string dialogueName, string textureName, bool[] collectedStats)
        {
            _dialogueName = dialogueName;
            _textureName = textureName;

            _collectedStats = collectedStats;
            _items = new UIEntity[collectedStats.Length];
            SetItemGrid(collectedStats);
        }

        public override void HandleInput()
        {
            if (InDialogueMode)
            {
                return;
            }

            bool moved = false;


            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
            {
                if (_selectedID % 4 == 3)
                {
                    return;
                }
                else if (_selectedID == _items.Length - 1)
                {
                    return;
                }

                _selectedID++;

                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
            {
                if (_selectedID % 4 == 0)
                {
                    ExitSubState();
                    return;
                }
                else
                {
                    _selectedID--;
                }

                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                if (_selectedID < 4)
                {
                    return;
                }

                _selectedID -= 4;

                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {

                if (_selectedID > _items.Length - 5)
                {
                    _selectedID = _items.Length - 1;
                }
                else
                {
                    _selectedID += 4;
                }

                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
            {
                ExitSubState();
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                if (_collectedStats[_selectedID])
                {
                    SetDialogue(DialogueManager.GetDialogue("misc", "any", _dialogueName, _selectedID));
                }

            }

            if (moved)
            {
                SoundManager.PlaySoundEffect("menu_move");
            }

            SetCursor();
        }

        public override void GetControl()
        {
            base.GetControl();

            _selectedID = 0;
            SetCursor();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            foreach (var item in _items)
            {
                item.Draw();
            }
        }

        private void SetCursor()
        {
            Vector2 pos = _items[_selectedID].Position + new Vector2(-8, 5);

            _selector.Position = pos;
        }

        private void SetItemGrid(bool[] collectedStats)
        {
            Vector2 startPos = new Vector2(68, 28);
            Vector2 itemSize = new Vector2(16 + 6);

            for (int i = 0; i < collectedStats.Length; i++)
            {
                int frame = collectedStats[i] ? i : collectedStats.Length;
                _items[i] = new UIEntity(startPos + new Vector2(i % 4, i / 4) * itemSize, _textureName, frame, 16, 16, DrawOrder.EQUIPMENT_ICON);
            }
        }
    }
}
