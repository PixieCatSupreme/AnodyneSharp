using AnodyneSharp.Registry;
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
        }

    }
    public static class Cheatz
    {
        [Cheat("LRLR121212")]
        public static void ToggleFuckItMode()
        {
            GlobalState.FUCK_IT_MODE_ON = !GlobalState.FUCK_IT_MODE_ON;
        }

        [Cheat("UUDDLRLR21")]
        public static void KonamiCode()
        {
            GlobalState.CUR_HEALTH = GlobalState.MAX_HEALTH;
        }

        [Cheat("URLDURLD11")]
        public static void ToggleForceCellSprite()
        {
        }

        [Cheat("URDLURDLUR")]
        public static void ToggleChaosMode()
        {
        }
    }
}
