using AnodyneSharp.Entities;
using AnodyneSharp.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Registry
{
    public class InventoryManager
    {
        //Inventory stuff

        public bool HasAnyBroom
        {
            get
            {
                return HasBroom || HasLengthen || HasWiden || HasTransformer;
            }
        }

        public bool HasEveryBroom
        {
            get
            {
                return HasBroom && HasLengthen && HasWiden && HasTransformer;
            }
        }

        public bool HasBroomType(BroomType type)
        {
            return type switch
            {
                BroomType.Normal => HasBroom,
                BroomType.Long => HasLengthen,
                BroomType.Wide => HasWiden,
                BroomType.Transformer => HasTransformer,
                _ => false,
            };
        }

        public BroomType EquippedBroom
        {
            get
            {
                if (!GlobalState.CanChangeBroom && _equippedBroom != BroomType.Transformer && _equippedBroom != BroomType.NONE)
                    return BroomType.Normal;

                return _equippedBroom;
            }
            set
            {
                if (HasBroomType(value))
                {
                    BroomType old_val = _equippedBroom;
                    if (!GlobalState.CanChangeBroom && value != BroomType.Transformer)
                    {
                        _equippedBroom = BroomType.Normal;
                    }
                    else
                    {
                        _equippedBroom = value;
                    }
                    EquippedBroomChanged = _equippedBroom != old_val;
                }
            }
        }

        public int CardCount
        {
            get
            {
                return CardStatus.Count(c => c);
            }
        }

        public bool UnlockedSecretz
        {
            get
            {
                return SecretStatus.Any(s => s);
            }
        }

        public bool UnlockedAllSecretz
        {
            get
            {
                return SecretStatus.All(s => s);
            }
        }

        public bool EquippedBroomChanged = false;

        public bool HasBroom = false;
        public bool HasLengthen = false;
        public bool HasWiden = false;
        public bool HasTransformer = false;

        public bool CanJump = false;

        public enum TradeState
        {
            NONE,
            SHOES,
            BOX
        };

        public TradeState tradeState = TradeState.NONE;

        public bool[] CardStatus = Enumerable.Repeat(false, 49).ToArray();
        public bool[] SecretStatus = Enumerable.Repeat(false, 14).ToArray();
        public bool[] BigKeyStatus = Enumerable.Repeat(false, 3).ToArray();

        public Dictionary<string, int> _mapKeys = new();
        public BroomType _equippedBroom = BroomType.NONE;

        public int GetCurrentMapKeys()
        {
            return GetMapKeys(GlobalState.CURRENT_MAP_NAME);
        }

        public int GetMapKeys(string mapName)
        {
            if (!_mapKeys.ContainsKey(mapName))
            {
                return 0;
            }

            return _mapKeys[mapName];
        }

        public bool SetMapKeys(string mapName, int count)
        {
            if (!_mapKeys.ContainsKey(mapName))
            {
                return false;
            }

            _mapKeys[mapName] = count;
            return true;
        }

        public int AddCurrentMapKey()
        {
            return AddMapKey(GlobalState.CURRENT_MAP_NAME, 1);
        }

        public int RemoveCurrentMapKey()
        {
            return AddMapKey(GlobalState.CURRENT_MAP_NAME, -1);
        }

        public int AddMapKey(string mapName, int addition)
        {
            GlobalState.RefreshKeyCount = true;

            if (!_mapKeys.ContainsKey(mapName))
            {
                _mapKeys.Add(mapName, addition);
            }
            else
            {
                _mapKeys[mapName] += addition;
            }

            DebugLogger.AddInfo($"Set key count of {mapName} to {_mapKeys[mapName]}");

            return _mapKeys[mapName];
        }
    }
}
