using AnodyneSharp.Entities;
using System.Collections.Generic;

namespace AnodyneSharp.Registry
{
    public static class InventoryState
    {
        //Inventory stuff

        public static bool HasAnyBroom
        {
            get
            {
                return HasBroom || HasLenghten || HasWiden || HasTransformer;
            }
        }

        public static BroomType EquippedBroom
        {
            get
            {
                return _equippedBroom;
            }
            set
            {
                _equippedBroom = value;
                EquippedBroomChanged = true;
            }
        }

        public static bool HasBroom = false;
        public static bool HasLenghten = false;
        public static bool HasWiden = false;
        public static bool HasTransformer = false;

        public static bool CanJump = false;
        public static bool UnlockedSecretz = false;

        public static bool EquippedBroomChanged = false;

        private static Dictionary<string, int> _mapKeys;
        private static BroomType _equippedBroom = BroomType.NONE;

        static InventoryState()
        {
            _mapKeys = new Dictionary<string, int>();
        }

        public static int GetCurrentMapKeys()
        {
            return GetMapKeys(GlobalState.CURRENT_MAP_NAME);
        }

        public static int GetMapKeys(string mapName)
        {
            if (!_mapKeys.ContainsKey(mapName))
            {
                return 0;
            }

            return _mapKeys[mapName];
        }

        public static int AddCurrentMapKey()
        {
            return AddMapKey(GlobalState.CURRENT_MAP_NAME, 1);
        }

        public static int RemoveCurrentMapKey()
        {
            return AddMapKey(GlobalState.CURRENT_MAP_NAME, -1);
        }

        public static int AddMapKey(string mapName, int addition)
        {
            if (!_mapKeys.ContainsKey(mapName))
            {
                _mapKeys.Add(mapName, addition);
            }
            else
            {
                _mapKeys[mapName] += addition;
            }

            return _mapKeys[mapName];
        }
    }
}
