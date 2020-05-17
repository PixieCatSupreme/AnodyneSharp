using AnodyneSharp.Entities;
using AnodyneSharp.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Registry
{
    public enum Achievements
    {
        NONE = -1,
        CardGet,            //"Got A Card! It's your first one."
        Cards7,             //"Found Seven Cards! Lucky number 7!"
        HpUp,               //"Health up! You feel stronger already!"
        FastFields,         //"Fast Fields. Reached the Fields in 10 minutes."
        NoHitSeer,          //"No Damage. Defeated the Seer without getting hit."
        Secret,             //"Mega secret. Got a secret!"
        GetBroom,           //"A Tool. You found the broom!"
        WindmillCard,       //"Change. You activated the windmill!"
        Briar,              //"A Meeting. You defeated Briar!"
        HundredPercent,     //"100% You finished the game with over 37 cards, all the health and broom upgrades!" (37 cards, all upgrades)
        TwoHundredPercent,  //"200% Well aren't you the curious one." (49 cards, all upgrades)
        ColorCubes,         //"Got all colored cubes! You must be very patient..."
        BwCubes,            //"Got the Black and White cubes! Just as on T.V.!"
        Cubes,              //"Got all the cubes! That's all five of them."
        Cards48,            //"Got 48 cards! That's quite the collection."
        Cards49,            //"Got the 49th card! That's all Folks!"
        Sub15MinutesEnd,    //"Beat the Briar under 15 minutes! As teased in the DEBUG WORLD."
        Sub3Hour100Percent  //"100% the game under 3 hours. You 100%'d and finished the game in under 3 hours!"
    }

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

        public static int CardCount
        {
            get
            {
                return CardStatus.Count(c => c);
            }
        }

        public static bool UnlockedSecretz
        {
            get
            {
                return SecretStatus.Any(s => s);
            }
        }

        public static bool EquippedBroomChanged = false;

        public static bool HasBroom = false;
        public static bool HasLenghten = false;
        public static bool HasWiden = false;
        public static bool HasTransformer = false;

        public static bool CanJump = false;

        public static bool[] CardStatus;
        public static bool[] SecretStatus;
        public static bool[] AchievementStatus;

        private static Dictionary<string, int> _mapKeys;
        private static BroomType _equippedBroom = BroomType.NONE;

        static InventoryState()
        {
            ResetValues();
        }

        public static void ResetValues()
        {
            DebugLogger.AddInfo("Setting inventory progress to default");

            _mapKeys = new Dictionary<string, int>();

            CardStatus = Enumerable.Repeat(false, 49).ToArray();
            SecretStatus = Enumerable.Repeat(false, 13).ToArray();
            AchievementStatus = Enumerable.Repeat(false, 18).ToArray();

            HasBroom = false;
            HasLenghten = false;
            HasWiden = false;
            HasTransformer = false;

            CanJump = false;
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
