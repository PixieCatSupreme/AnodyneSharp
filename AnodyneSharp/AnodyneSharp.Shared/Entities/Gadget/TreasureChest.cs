using AnodyneSharp.Entities.Gadget.Treasures;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("Treasure"), Collision(typeof(Player))]
    public class TreasureChest : Entity
    {
        private enum TreasureType
        {
            NONE = -1,
            BROOM,
            KEY,
            GROWTH,
            JUMP,
            WIDE,
            LONG,
            SWAP,
            SECRET
        }

        EntityPreset _preset;
        BaseTreasure _treasure;
        TreasureType _treasureType;

        public bool opened;

        public TreasureChest(EntityPreset preset)
            : base(preset.Position, "treasureboxes", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            int frame = 0;

            _preset = preset;
            immovable = true;

            if (GlobalState.CURRENT_MAP_NAME == "TRAIN")
            {
                frame = 4;
            }

            if (preset.Frame == -1)
            {
                frame++;
                opened = true;
            }
            else
            {
                SetTreasure();
            }

            SetFrame(frame);
        }

        public override void Update()
        {
            base.Update();

            if (!opened && touching == Touching.DOWN && KeyInput.JustPressedKey(Keys.V))
            {
                opened = true;
                _treasure.GetTreasure();

                SetFrame(_curFrame + 1);
                _preset.Frame = -1;
            }

            if (opened && _treasure != null)
            {
                _treasure.Update();
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (opened && _treasure != null)
            {
                _treasure.Draw();
            }
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        private void SetTreasureType()
        {
            _treasureType = TreasureType.NONE;

            switch (_preset.Frame)
            {
                case 0:
                case 4:
                case 5:
                case 6:
                    _treasureType = TreasureType.BROOM;
                    break;
                case 1:
                    _treasureType = TreasureType.KEY;
                    break;
                case 2:
                    _treasureType = TreasureType.GROWTH;
                    break;
                default:
                    if (_preset.Frame >= 8 && _preset.Frame <= 20)
                    {
                        _treasureType = TreasureType.SECRET;
                    }
                    break;

            }
        }

        private void SetTreasure()
        {
            SetTreasureType();

            switch (_treasureType)
            {
                case TreasureType.BROOM:
                    _treasure = new BroomTreasure("broom-icon", Position, BroomType.Normal);
                    break;
                case TreasureType.KEY:
                    _treasure = new KeyTreasure(Position);
                    break;
                case TreasureType.GROWTH:
                    int id = CardDataManager.GetCardId();

                    if (id != -1)
                    {
                        _treasure = new CardTreasure(Position, id);
                    }
                    else
                    {
                        FailsafeTreasure();
                    }

                    break;
                case TreasureType.JUMP:
                    _treasure = new BootsTreasure(Position);
                    break;
                case TreasureType.WIDE:
                    _treasure = new BroomTreasure("item_wide_attack", Position, BroomType.Wide);
                    break;
                case TreasureType.LONG:
                    _treasure = new BroomTreasure("item_long_attack", Position, BroomType.Long);
                    break;
                case TreasureType.SWAP:
                    _treasure = new BroomTreasure("item_tranformer", Position, BroomType.Transformer);
                    break;
                case TreasureType.SECRET:
                    _treasure = new SecretTreasure(Position, _preset.Frame - 7, _preset.Frame == 10 ? 9 : -1);
                    break;
                default:
                    FailsafeTreasure();
                    break;
            }
        }

        private void FailsafeTreasure()
        {
            _treasure = new Treasure("PixieCatSupreme", Position, 0, -2);
        }
    }
}
