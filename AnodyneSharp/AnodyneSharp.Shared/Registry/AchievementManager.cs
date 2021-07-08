using AnodyneSharp.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Registry
{
    public enum AchievementValue
    {
        NONE = -1,
        GetBroom,           //"A Tool. You found the broom!"
        CardGet,            //"Got A Card! It's your first one."
        Cards7,             //"Found Seven Cards! Lucky number 7!"
        Cards48,            //"Got 48 cards! That's quite the collection."
        Cards49,            //"Got the 49th card! That's all Folks!"
        HpUp,               //"Health up! You feel stronger already!"
        WindmillCard,       //"Change. You activated the windmill!"
        Briar,              //"A Meeting. You defeated Briar!"
        NoHitSeer,          //"No Damage. Defeated the Seer without getting hit."
        Secret,             //"Mega secret. Got a secret!"
        ColorCubes,         //"Got all colored cubes! You must be very patient..."
        BwCubes,            //"Got the Black and White cubes! Just as on T.V.!"
        HundredPercent,     //"100% You finished the game with over 37 cards, all the health and broom upgrades!" (37 cards, all upgrades)
        TwoHundredPercent,  //"200% Well aren't you the curious one." (49 cards, all upgrades)
        FastFields,         //"Fast Fields. Reached the Fields in 10 minutes."
        Sub15MinutesEnd,    //"Beat the Briar under 15 minutes! Wiggle your way to success."
        Sub3Hour100Percent  //"100% the game under 3 hours. You 100%'d and finished the game in under 3 hours!"
    }

    public class AchievementManager
    {
        public bool[] AchievementStatus = Enumerable.Repeat(false, 18).ToArray();
        public bool AchievementsDisabled = false;

        public void CheckCardAchievements()
        {
            if (!AchievementsDisabled)
            {
                int cc = GlobalState.inventory.CardCount;

                if (cc >= 1)
                {
                    UnlockAchievement(AchievementValue.CardGet);
                }
                if (cc >= 7)
                {
                    UnlockAchievement(AchievementValue.Cards7);
                }
                if (cc >= 48)
                {
                    UnlockAchievement(AchievementValue.Cards48);
                }
                if (cc >= 49)
                {
                    UnlockAchievement(AchievementValue.Cards49);
                }
            }
        }

        //Will only check after defeating Briar
        public void CheckInventoryAchievements()
        {
            if (!AchievementsDisabled)
            {
                int cc = GlobalState.inventory.CardCount;

                if (GlobalState.MAX_HEALTH == 16 && GlobalState.inventory.HasEveryBroom)
                {
                    if (cc >= 37)
                    {
                        UnlockAchievement(AchievementValue.HundredPercent);
                    }
                    if (cc >= 49 && GlobalState.inventory.UnlockedAllSecretz)
                    {
                        UnlockAchievement(AchievementValue.TwoHundredPercent);
                    }
                }
            }
        }

        public void CheckCubeAchievements()
        {
            if (!AchievementsDisabled)
            {
                bool hasColorCubes = GlobalState.inventory.SecretStatus[9] && GlobalState.inventory.SecretStatus[10] && GlobalState.inventory.SecretStatus[11];
                bool hasBwCubes = GlobalState.inventory.SecretStatus[8] && GlobalState.inventory.SecretStatus[12];

                if (hasColorCubes)
                {
                    UnlockAchievement(AchievementValue.ColorCubes);
                }

                if (hasBwCubes)
                {
                    UnlockAchievement(AchievementValue.BwCubes);
                }
            }
        }

        public void UnlockAchievement(AchievementValue achievement)
        {
            int id = (int)achievement;

            if (!AchievementStatus[id])
            {
                //TODO maybe play sound?

                AchievementStatus[id] = true;
            }
        }
    }
}
