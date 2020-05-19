using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using AnodyneSharp.UI.PauseMenu.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class CardSubstate : DialogueSubstate
    {
        private int page;
        private int selectedID;

        private UIEntity[] cards;

        private TextSelector _pageSetter;

        private UILabel cardsLabel;

        public CardSubstate()
        {
            cards = new UIEntity[12];

            page = 0;
            selectedID = 0;

            cardsLabel = new UILabel(new Vector2(70, 146), true);
            cardsLabel.Initialize();
            cardsLabel.SetText($"{InventoryManager.CardCount} cards");

            _pageSetter = new TextSelector(new Vector2(91, 156), 32, 0, "1/4", "2/4", "3/4", "4/4")
            {
                ValueChangedEvent = PageValueChanged,
                noConfirm = true,
                noLoop = true
            };

            SetCardPage();
        }

        public override void HandleInput()
        {
            if (InDialogueMode)
            {
                return;
            }

            bool moved = false;

            if (_pageSetter.Enabled)
            {
                if (KeyInput.CanPressKey(Keys.Up))
                {
                    _pageSetter.Enabled = false;
                    _selector.visible = true;
                    SoundManager.PlaySoundEffect("menu_select");
                    return;
                }

                _pageSetter.Update();
            }

            if (KeyInput.CanPressKey(Keys.Right))
            {
                if (selectedID % 3 == 2)
                {
                    if (page == 3)
                    {
                        return;
                    }

                    selectedID -= 2;
                    page++;
                    SetCardPage();
                }
                else
                {
                    selectedID++;

                }

                moved = true;
            }
            else if (KeyInput.CanPressKey(Keys.Left))
            {
                if (selectedID % 3 == 0)
                {
                    if (page == 0)
                    {
                        ExitSubState();
                        return;
                    }

                    selectedID += 2;
                    page--;
                    SetCardPage();
                }
                else
                {
                    selectedID--;
                }

                moved = true;
            }
            else if (KeyInput.CanPressKey(Keys.Up))
            {
                if (selectedID < 3)
                {
                    return;
                }

                selectedID -= 3;

                moved = true;
            }
            else if (KeyInput.CanPressKey(Keys.Down))
            {

                if (selectedID > 8)
                {
                    if (!_pageSetter.Enabled)
                    {
                        _pageSetter.Enabled = true;
                        _selector.visible = false;
                        SoundManager.PlaySoundEffect("menu_select");
                    }
                    return;
                }

                selectedID += 3;

                moved = true;
            }
            else if (KeyInput.CanPressKey(Keys.X))
            {
                ExitSubState();
            }
            else if (KeyInput.CanPressKey(Keys.C))
            {
                int cardID = page * 12 + selectedID;
                if (InventoryManager.CardStatus[cardID])
                {
                    SetDialogue($"card text {cardID}");
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

            selectedID = 0;
            SetCursor();
        }

        public override void DrawUI()
        {
            base.DrawUI();

            foreach (var card in cards)
            {
                card.Draw();
            }

            _pageSetter.DrawUI();
            cardsLabel.Draw();
        }

        private void SetCursor()
        {
            Vector2 pos = cards[selectedID].Position + new Vector2(-10, 8);

            _selector.Position = pos;
        }

        private void SetCardPage()
        {
            Vector2 startPos = new Vector2(68, 28);
            Vector2 cardSize = new Vector2(24 + 6);

            for (int i = 0; i < 12; i++)
            {
                int cardnum = page * 12 + i;
                int frame = InventoryManager.CardStatus[cardnum] ? cardnum : 49;
                cards[i] = new UIEntity(startPos + new Vector2(i % 3, i / 3) * cardSize, "card_sheet", frame, 24, 24, DrawOrder.EQUIPMENT_ICON);
            }

            _pageSetter.SetValue(page);
        }

        private void PageValueChanged(string value, int index)
        {
            page = index;
            SetCardPage();
        }
    }
}
