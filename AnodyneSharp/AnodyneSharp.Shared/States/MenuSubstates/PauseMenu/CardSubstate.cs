using AnodyneSharp.Dialogue;
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

namespace AnodyneSharp.States.MenuSubstates
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

            cardsLabel = new UILabel(new Vector2(70, 146 - GameConstants.LineOffset - (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0)), true, $"{GlobalState.inventory.CardCount} {DialogueManager.GetDialogue("misc", "any", "cards", 1)}");

            _pageSetter = new TextSelector(new Vector2(91, 156), 32, 0, true, "1/4", "2/4", "3/4", "4/4")
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

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
            {
                _pageSetter.LoseControl();
                ExitSubState();
                return;
            }

            if (selectedID < 0)
            {
                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
                {
                    _pageSetter.LoseControl();
                    selectedID = -selectedID;
                    selector.visible = true;
                    SoundManager.PlaySoundEffect("menu_select");
                }
                else
                {
                    _pageSetter.Update();
                }

                return;
            }

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
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
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
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
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                if (selectedID < 3)
                {
                    return;
                }

                selectedID -= 3;

                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {

                if (selectedID > 8)
                {
                    _pageSetter.GetControl();
                    selector.visible = false;
                    selectedID = -selectedID;
                    SoundManager.PlaySoundEffect("menu_select");
                    return;
                }

                selectedID += 3;

                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                int cardID = page * 12 + selectedID;
                if (GlobalState.inventory.CardStatus[cardID])
                {
                    SetDialogue(DialogueManager.GetDialogue("card", "ETC", "one", cardID));
                }
            }
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.NextPage))
            {
                if (page == 3)
                {
                    return;
                }

                page++;
                SetCardPage();

                moved = true;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.PreviousPage))
            {
                if (page == 0)
                {
                    return;
                }

                page--;
                SetCardPage();

                moved = true;
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

            _pageSetter.Draw();
            cardsLabel.Draw();
        }

        private void SetCursor()
        {
            Vector2 pos = cards[selectedID].Position + new Vector2(-10, 8);

            selector.Position = pos;
        }

        private void SetCardPage()
        {
            Vector2 startPos = new Vector2(68, 28);
            Vector2 cardSize = new Vector2(24 + 6);

            for (int i = 0; i < 12; i++)
            {
                int cardnum = page * 12 + i;
                int frame = GlobalState.inventory.CardStatus[cardnum] ? cardnum : 49;
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
