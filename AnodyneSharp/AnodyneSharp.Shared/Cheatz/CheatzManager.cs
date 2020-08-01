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
            AchievementManager.AchievementsDisabled = true;
#endif
        }

        [Cheat("UUDDLRLR21")]
        public static void KonamiCode()
        {
            SoundManager.PlaySoundEffect("unlock");

            GlobalState.MAX_HEALTH = 16;
            GlobalState.CUR_HEALTH = GlobalState.MAX_HEALTH;

            GlobalState.RefreshMaxHealth = true;

            InventoryManager.HasBroom = true;
            InventoryManager.HasLenghten = true;
            InventoryManager.HasWiden = true;
            InventoryManager.HasTransformer = true;
            InventoryManager.CanJump = true;

#if RELEASE
            AchievementManager.AchievementsDisabled = true;
#endif

            for (int i = 0; i < InventoryManager.CardStatus.Length; i++)
            {
                InventoryManager.CardStatus[i] = true;
            }

            for (int i = 0; i < InventoryManager.SecretStatus.Length; i++)
            {
                InventoryManager.SecretStatus[i] = true;
            }

            InventoryManager.EquippedBroom = InventoryManager.EquippedBroom == Entities.BroomType.NONE ? Entities.BroomType.Normal : InventoryManager.EquippedBroom;
        }

        [Cheat("UDUD121212")]
        public static void UnlockNexusGates()
        {
            for (int i = 0; i < GlobalState.ActivatedNexusPortals.Length; i++)
            {
                GlobalState.ActivatedNexusPortals[i] = true;
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
            InventoryManager.AddCurrentMapKey();
            SoundManager.PlaySoundEffect("keyget");
            GlobalState.RefreshKeyCount = true;

            AchievementManager.AchievementsDisabled = true;
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

        [Cheat("1212121212")]
        public static void Meow()
        {
            SoundManager.PlaySoundEffect("meow");

#if DEBUG
            for (int i = 0; i < AchievementManager.AchievementStatus.Length; i++)
            {
                AchievementManager.AchievementStatus[i] = true;
            }
#endif
        }

        [Cheat("2121212121")]
        public static void Woof()
        {
            SoundManager.PlaySoundEffect("woof");
        }
    }
}
