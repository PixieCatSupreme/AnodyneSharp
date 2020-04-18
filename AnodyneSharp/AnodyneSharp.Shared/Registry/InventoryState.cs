using AnodyneSharp.Entities;
using System.Collections.Generic;

namespace AnodyneSharp.Registry
{
    public static class InventoryState
    {
        //Inventory stuff

        public static BroomType EquippedBroom = BroomType.NONE;
        //TODO add check for which broom types are picked up
        public static bool CanJump;

        private static Dictionary<string, int> _mapKeys;

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
