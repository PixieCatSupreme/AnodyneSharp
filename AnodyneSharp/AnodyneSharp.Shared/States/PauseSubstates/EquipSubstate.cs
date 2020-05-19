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
using System.Text;

namespace AnodyneSharp.States.PauseSubstates
{
    public class EquipSubstate : PauseSubstate
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

        private EquipState _state;
        private EquipState _lastState;

        public EquipSubstate()
        {
            float x = 65;
            float y = 25;

            _broom = new Equipment(new Vector2(x, y), "none_icon", InventoryManager.HasBroom ? "Normal" : "-");
            _broomExtend = new Equipment(new Vector2(x, y + 24), "long_icon", InventoryManager.HasLenghten ? "Extend" : "-");
            _broomWiden = new Equipment(new Vector2(x, y + 24 * 2), "wide_icon", InventoryManager.HasWiden ? "Widen" : "-");
            _transformer = new Equipment(new Vector2(x, y + 24 * 3), "transformer_icon", InventoryManager.HasTransformer ? "Swap" : "-");


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
            if (_lastState != _state)
            {
                _lastState = _state;
                SetSelectorPos();
                SoundManager.PlaySoundEffect("menu_move");
            }

            base.Update();
        }

        public override void DrawUI()
        {
            _broom.Draw();
            _broomExtend.Draw();
            _broomWiden.Draw();
            _transformer.Draw();

            _selector.Draw();
        }

        public override void HandleInput()
        {
            if (KeyInput.JustPressedKey(Keys.Up))
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
            else if (KeyInput.JustPressedKey(Keys.Down))
            {
                if (_state >= EquipState.Item)
                {
                    return;
                }

                _state++;
            }
            else if (_state >= EquipState.Shoes && KeyInput.JustPressedKey(Keys.Right))
            {
                if (_state == EquipState.Key3)
                {
                    return;
                }

                _state++;
            }
            else if (_state > EquipState.Shoes && KeyInput.JustPressedKey(Keys.Left))
            {
                _state--;
            }
            else if (KeyInput.JustPressedKey(Keys.C))
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
                    break;
                case EquipState.Item:
                    break;
                case EquipState.Key1:
                    break;
                case EquipState.Key2:
                    break;
                case EquipState.Key3:
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
                    break;
                case EquipState.Item:
                    break;
                case EquipState.Key1:
                    break;
                case EquipState.Key2:
                    break;
                case EquipState.Key3:
                    break;
            }

            _selector.Position -= new Vector2(_selector.frameWidth, -2);
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
