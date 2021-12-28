using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnodyneSharp.Cheatz
{
    public static class CheatzManager
    {
        private static Dictionary<string, MethodInfo> cheatz;
        static CheatzManager()
        {
            cheatz = typeof(Cheatz).GetMethods().Where(m => m.IsDefined(typeof(CheatAttribute)))
                .ToDictionary(m => m.GetCustomAttribute<CheatAttribute>().cheatInput, m => m);
        }

        public static void DoCheat(string input)
        {
            if (cheatz.ContainsKey(input))
            {
                cheatz[input].Invoke(null, null);
            }
            else
            {
                Default();
            }
        }

        public static void Default()
        {
            GlobalState.CUR_HEALTH -= 2;
            SoundManager.PlaySoundEffect("sb_hurt");
        }

    }
    public static class Cheatz
    {
        [Cheat("LRLR121212")]
        public static void ToggleFuckItMode()
        {
            GlobalState.FUCK_IT_MODE_ON = !GlobalState.FUCK_IT_MODE_ON;

#if RELEASE
            GlobalState.achievements.AchievementsDisabled = true;
#endif
        }

        [Cheat("UUDDLRLR21")]
        public static void KonamiCode()
        {
            SoundManager.PlaySoundEffect("unlock");

            GlobalState.MAX_HEALTH = 16;
            GlobalState.CUR_HEALTH = GlobalState.MAX_HEALTH;

            GlobalState.inventory.HasBroom = true;
            GlobalState.inventory.HasLengthen = true;
            GlobalState.inventory.HasWiden = true;
            GlobalState.inventory.HasTransformer = true;
            GlobalState.inventory.CanJump = true;

#if RELEASE
            GlobalState.achievements.AchievementsDisabled = true;
#endif

            for (int i = 0; i < GlobalState.inventory.CardStatus.Length; i++)
            {
                GlobalState.inventory.CardStatus[i] = true;
            }

            for (int i = 0; i < GlobalState.inventory.SecretStatus.Length; i++)
            {
                GlobalState.inventory.SecretStatus[i] = true;
            }

            GlobalState.inventory.EquippedBroom = GlobalState.inventory.EquippedBroom == Entities.BroomType.NONE ? Entities.BroomType.Normal : GlobalState.inventory.EquippedBroom;
        }

        [Cheat("UDUD121212")]
        public static void UnlockNexusGates()
        {
            foreach(var map in (GameConstants.MapOrder[])Enum.GetValues(typeof(GameConstants.MapOrder)))
            {
                GlobalState.events.ActivatedNexusPortals.Add(map.ToString());
            }

            SoundManager.PlaySoundEffect("menu_select");
            SoundManager.PlaySoundEffect("keyget");
        }

        [Cheat("URLDURLD11")]
        public static void ToggleAlwaysCellGraphics()
        {
            GlobalState.AlwaysCellGraphics = !GlobalState.AlwaysCellGraphics;
            GlobalState.ForceTextureReload = true;
        }

        [Cheat("12UULLRRDD")]
        public static void GiveKey()
        {
            GlobalState.inventory.AddCurrentMapKey();
            SoundManager.PlaySoundEffect("keyget");
            GlobalState.RefreshKeyCount = true;
        }

        [Cheat("ULRDDLRU12")]
        public static void ResetMap()
        {
            //TODO reset map cheat
        }

        [Cheat("URDLURDLUR")]
        public static void ToggleChaosMode()
        {
            GlobalState.GameMode = GlobalState.GameMode == GameMode.Chaos ? GameMode.Normal : GameMode.Chaos;
            GlobalState.ForceTextureReload = true;

            if (GlobalState.GameMode == GameMode.Chaos)
            {
                SoundManager.PlaySoundEffect("shieldy-hit");
                SoundManager.PlaySoundEffect("small_wave");
                SoundManager.PlaySoundEffect("big_door_locked");
            }
        }

        [Cheat("ULDRULDRUL")]
        public static void ToggleExtremeChaosMode()
        {
            GlobalState.GameMode = GlobalState.GameMode == GameMode.EXTREME_CHAOS ? GameMode.Normal : GameMode.EXTREME_CHAOS;

            GlobalState.ForceTextureReload = true;

            if (GlobalState.GameMode == GameMode.EXTREME_CHAOS)
            {
                SoundManager.PlaySoundEffect("shieldy-hit");
                SoundManager.PlaySoundEffect("small_wave");
                SoundManager.PlaySoundEffect("big_door_locked");
                SoundManager.PlaySoundEffect("fall_in_hole");
            }
        }
    }
}
