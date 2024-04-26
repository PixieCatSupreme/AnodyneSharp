using AnodyneSharp.Archipelago;
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
    public class TreasureChest : Entity, Interactable
    {
        public enum TreasureType
        {
            NONE = -1,
            BROOM,
            KEY,
            GROWTH,
            JUMP,
            WIDE,
            LONG,
            SWAP,
            SECRET,
            ARCHIPELAGO = 21
        }

        private int _frame;
        private string _typeValue;
        private int? _locationID;

        private EntityPreset _preset;
        private BaseTreasure _treasure;

        private TreasureType _treasureType;

        public bool opened;

        public TreasureChest(EntityPreset preset, Player p)
            : base(preset.Position, "treasureboxes", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            int frame = 0;

            _preset = preset;
            _frame = preset.Frame;
            _typeValue = preset.TypeValue;

            immovable = true;

            if (GlobalState.IsCell)
            {
                frame = 4;
            }

            if (preset.Activated)
            {
                frame++;
                opened = true;
            }
            else
            {
                GlobalState.CurrentMinimap.AddInterest();
                SetTreasure();
            }

            SetFrame(frame);
        }

        public override void Update()
        {
            base.Update();

            if (opened && _treasure != null)
            {
                _treasure.Update();
            }
        }

        public virtual bool PlayerInteraction(Facing player_direction)
        {
            if(opened || player_direction != Facing.UP)
            {
                return false;
            }
            opened = true;
            GlobalState.CurrentMinimap.RemoveInterest();
            _treasure.GetTreasure();

            SetFrame(Frame + 1);
            _preset.Activated = true;
            return true;
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

            if (ArchipelagoSessionHandler.IsConnected)
            {
                int? locationID = GlobalState.GetLocationID(_preset.EntityID);

                if (locationID != null)
                {
                    Item item = ArchipelagoSessionHandler.GetItemAtLocation(locationID.Value);

                    _frame = item.Frame;
                    _typeValue = item.TypeValue;
                }
            }

            switch (_frame)
            {
                case 0:
                    _treasureType = TreasureType.BROOM;
                    break;
                case 4:
                    _treasureType = TreasureType.WIDE;
                    break;
                case 5:
                    _treasureType = TreasureType.LONG;
                    break;
                case 6:
                    _treasureType = TreasureType.SWAP;
                    break;
                case 1:
                    _treasureType = TreasureType.KEY;
                    break;
                case 2:
                    _treasureType = TreasureType.GROWTH;
                    break;
                case 21:
                    _treasureType = TreasureType.ARCHIPELAGO;
                    break;
                default:
                    if (_preset.Frame >= 7 && _preset.Frame <= 20)
                    {
                        _treasureType = TreasureType.SECRET;
                    }
                    break;
            }
        }

        private void SetTreasure()
        {
            SetTreasureType();

            if (_treasureType == TreasureType.GROWTH && int.TryParse(_preset.TypeValue, out int cardID))
            {
                _treasure = new CardTreasure(Position, cardID);
                return;
            }

            switch (_treasureType)
            {
                case TreasureType.BROOM:
                    _treasure = new BroomTreasure("broom-icon", Position, BroomType.Normal);
                    break;
                case TreasureType.KEY:
                    _treasure = new KeyTreasure(Position, _typeValue);
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
                    _treasure = new SecretTreasure(Position, _frame - 7, _frame == 10 ? 7 : -1);
                    break;
                case TreasureType.ARCHIPELAGO:
                    if (_locationID == null)
                    {
                        FailsafeTreasure();
                        return;
                    }

                    _treasure = new ArchipelagoTreasure(Position, ArchipelagoSessionHandler.GetOutsideItemInfo(_locationID.Value));
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

    [NamedEntity("Treasure",map:"WINDMILL")]
    class WindmillTreasureChest : TreasureChest
    {
        public WindmillTreasureChest(EntityPreset preset, Player p) : base(preset,p)
        {

        }

        public override bool PlayerInteraction(Facing player_direction)
        {
            if(GlobalState.events.GetEvent("WindmillOpened") == 0)
            {
                GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("misc", "any", "treasure", 0);
                return true;
            }
            return base.PlayerInteraction(player_direction);
        }
    }
}
