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

        private MenuSelector _selector;

        private EquipState _state;
        private EquipState _lastState;

        public EquipSubstate()
        {
            float x = 65;
            float y = 25;

            _broom = new Equipment(new Vector2(x, y), "none_icon", InventoryState.HasBroom ? "Normal" : "-");
            _broomExtend = new Equipment(new Vector2(x, y + 24), "long_icon", InventoryState.HasLenghten ? "Extend" : "-");
            _broomWiden = new Equipment(new Vector2(x, y + 24 * 2), "wide_icon", InventoryState.HasWiden ? "Widen" : "-");
            _transformer = new Equipment(new Vector2(x, y + 24 * 3), "transformer_icon", InventoryState.HasTransformer ? "Swap" : "-");

            _selector = new MenuSelector(Vector2.Zero)
            {
                visible = false
            };

            SetEquipped();
        }

        public override void GetControl()
        {
            if (!InventoryState.HasAnyBroom)
            {
                Exit = true;
                return;
            }

            _selector.visible = true;
            _selector.Play("enabledRight");
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

            _selector.Update();
            _selector.PostUpdate();
        }

        public override void DrawUI()
        {
            _broom.Draw();
            _broomExtend.Draw();
            _broomWiden.Draw();
            _transformer.Draw();

            _selector.Draw();
        }

        protected override void ExitSubState()
        {
            base.ExitSubState();
            _selector.visible = false;
        }

        public override void HandleInput()
        {
            if (KeyInput.CanPressKey(Keys.Up))
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
            else if (KeyInput.CanPressKey(Keys.Down))
            {
                if (_state >= EquipState.Item)
                {
                    return;
                }

                _state++;
            }
            else if (_state >= EquipState.Shoes && KeyInput.CanPressKey(Keys.Right))
            {
                if (_state == EquipState.Key3)
                {
                    return;
                }

                _state++;
            }
            else if (_state > EquipState.Shoes && KeyInput.CanPressKey(Keys.Left))
            {
                _state--;
            }
            else if (KeyInput.CanPressKey(Keys.C))
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
            InventoryState.EquippedBroom = broomType;
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

            switch (InventoryState.EquippedBroom)
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
