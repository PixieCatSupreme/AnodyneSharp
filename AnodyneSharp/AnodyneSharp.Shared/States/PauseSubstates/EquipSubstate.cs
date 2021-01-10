using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.PauseMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class EquipSubstate : DialogueSubstate
    {
        private enum EquipState
        {
            Broom,
            Extend,
            Widen,
            Transformer,
            Shoes,
            Item,
            Key1,
            Key2,
            Key3
        }

        private Equipment _broom;
        private Equipment _broomExtend;
        private Equipment _broomWiden;
        private Equipment _transformer;

        private UIEntity _jump;
        private UIEntity _item;
        private UIEntity[] _keys;

        private EquipState _state;
        private EquipState _lastState;

        List<EquipState> bottom_row_enabled = new() { EquipState.Key1, EquipState.Key2, EquipState.Key3 };
        int current_bottom_index = 0;

        public EquipSubstate()
        {
            float x = 65;
            float y = 25;

            _broom = new Equipment(new Vector2(x, y), "none_icon", InventoryManager.HasBroom ? DialogueManager.GetDialogue("misc", "any", "items", 1) : "-");
            _broomExtend = new Equipment(new Vector2(x, y + 24), "long_icon", InventoryManager.HasLenghten ? DialogueManager.GetDialogue("misc", "any", "items", 3) : "-");
            _broomWiden = new Equipment(new Vector2(x, y + 24 * 2), "wide_icon", InventoryManager.HasWiden ? DialogueManager.GetDialogue("misc", "any", "items", 4) : "-");
            _transformer = new Equipment(new Vector2(x, y + 24 * 3), "transformer_icon", InventoryManager.HasTransformer ? DialogueManager.GetDialogue("misc", "any", "items", 2) : "-");

            _jump = new UIEntity(new Vector2(62, 130), "item_jump_shoes", 0, 16, 16, Drawing.DrawOrder.EQUIPMENT_ICON) { visible = false };
            _item = new UIEntity(new Vector2(78, 130), "fields_npcs", 16, 16, Drawing.DrawOrder.EQUIPMENT_ICON) { visible = false };

            if (InventoryManager.tradeState != InventoryManager.TradeState.NONE)
            {
                bottom_row_enabled.Insert(0, EquipState.Item);
                _item.SetFrame(InventoryManager.tradeState == InventoryManager.TradeState.BOX ? 31 : 56);
                _item.visible = true;
            }

            if (InventoryManager.CanJump)
            {
                bottom_row_enabled.Insert(0, EquipState.Shoes);
                _jump.visible = true;
            }

            _keys = Enumerable.Range(0, 3).Select(i => new UIEntity(new Vector2(95 + 16 * i, 130), "key_green", InventoryManager.BigKeyStatus[i] ? i * 2 : i * 2 + 1, 16, 16, Drawing.DrawOrder.EQUIPMENT_ICON)).ToArray();

            SetEquipped();
        }

        public override void GetControl()
        {
            if (!InventoryManager.HasAnyBroom)
            {
                Exit = true;
                return;
            }

            base.GetControl();
            _state = EquipState.Broom;
            _lastState = _state;

            SetSelectorPos();
        }

        public override void Update()
        {
            base.Update();

            if (_lastState != _state)
            {
                _lastState = _state;
                SetSelectorPos();
                SoundManager.PlaySoundEffect("menu_move");
            }

        }

        public override void DrawUI()
        {
            base.DrawUI();
            _broom.Draw();
            _broomExtend.Draw();
            _broomWiden.Draw();
            _transformer.Draw();
            _item.Draw();
            _jump.Draw();
            foreach (var key in _keys)
            {
                key.Draw();
            }

            _selector.Draw();
        }

        public override void HandleInput()
        {
            if (InDialogueMode) return;

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Up))
            {
                if (_state == EquipState.Broom)
                {
                    return;
                }

                if (_state >= EquipState.Shoes)
                {
                    _state = EquipState.Transformer;
                    return;
                }

                _state--;
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Down))
            {
                if (_state >= EquipState.Item)
                {
                    return;
                }

                if (_state == EquipState.Transformer)
                {
                    _state = bottom_row_enabled[0];
                    current_bottom_index = 0;
                }
                else if (_state < EquipState.Transformer)
                {
                    _state++;
                }
            }
            else if (_state >= EquipState.Shoes && KeyInput.JustPressedRebindableKey(KeyFunctions.Right))
            {
                if (current_bottom_index == bottom_row_enabled.Count - 1)
                {
                    return;
                }

                _state = bottom_row_enabled[++current_bottom_index];
            }
            else if (_state > EquipState.Shoes && current_bottom_index > 0 && KeyInput.JustPressedRebindableKey(KeyFunctions.Left))
            {
                _state = bottom_row_enabled[--current_bottom_index];
            }
            else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept))
            {
                UseItem();
            }
            else
            {
                base.HandleInput();
            }
        }

        private void UseItem()
        {
            switch (_state)
            {
                case EquipState.Broom:
                    EquipBroom(BroomType.Normal);
                    break;
                case EquipState.Extend:
                    EquipBroom(BroomType.Long);
                    break;
                case EquipState.Widen:
                    EquipBroom(BroomType.Wide);
                    break;
                case EquipState.Transformer:
                    EquipBroom(BroomType.Transformer);
                    break;
                case EquipState.Shoes:
                    SetDialogue(DialogueManager.GetDialogue("misc", "any", "items", 5));
                    break;
                case EquipState.Item:
                    SetDialogue(DialogueManager.GetDialogue("misc", "any", "items", InventoryManager.tradeState == InventoryManager.TradeState.SHOES ? 6 : 7));
                    break;
                case EquipState.Key1:
                    if(InventoryManager.BigKeyStatus[0])
                    {
                        SetDialogue(DialogueManager.GetDialogue("misc", "any", "items", 8));
                    }
                    break;
                case EquipState.Key2:
                    if (InventoryManager.BigKeyStatus[1])
                    {
                        SetDialogue(DialogueManager.GetDialogue("misc", "any", "items", 9));
                    }
                    break;
                case EquipState.Key3:
                    if (InventoryManager.BigKeyStatus[2])
                    {
                        SetDialogue(DialogueManager.GetDialogue("misc", "any", "items", 10));
                    }
                    break;
            }
        }

        private void EquipBroom(BroomType broomType)
        {
            InventoryManager.EquippedBroom = broomType;
            SetEquipped();
            ExitSubState();
        }

        private void SetSelectorPos()
        {
            bool ignoreOffset = false;
            switch (_state)
            {
                case EquipState.Broom:
                    _selector.Position = _broom.LabelPos;
                    break;
                case EquipState.Extend:
                    _selector.Position = _broomExtend.LabelPos;
                    break;
                case EquipState.Widen:
                    _selector.Position = _broomWiden.LabelPos;
                    break;
                case EquipState.Transformer:
                    _selector.Position = _transformer.LabelPos;
                    break;
                case EquipState.Shoes:
                    ignoreOffset = true;
                    _selector.Position = _jump.Position;
                    break;
                case EquipState.Item:
                    ignoreOffset = true;
                    _selector.Position = _item.Position;
                    break;
                case EquipState.Key1:
                    ignoreOffset = true;
                    _selector.Position = _keys[0].Position;
                    break;
                case EquipState.Key2:
                    ignoreOffset = true;
                    _selector.Position = _keys[1].Position;
                    break;
                case EquipState.Key3:
                    ignoreOffset = true;
                    _selector.Position = _keys[2].Position;
                    break;
            }


            if (!ignoreOffset)
            {
                _selector.Position -= new Vector2(_selector.sprite.Width, -2);
                _selector.Position.Y += CursorOffset;
            }
        }

        private void SetEquipped()
        {
            _broom.equipped = false;
            _broomExtend.equipped = false;
            _broomWiden.equipped = false;
            _transformer.equipped = false;

            switch (InventoryManager.EquippedBroom)
            {
                case BroomType.Normal:
                    _broom.equipped = true;
                    break;
                case BroomType.Wide:
                    _broomWiden.equipped = true;
                    break;
                case BroomType.Long:
                    _broomExtend.equipped = true;
                    break;
                case BroomType.Transformer:
                    _transformer.equipped = true;
                    break;
            }
        }
    }
}
