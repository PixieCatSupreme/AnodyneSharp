using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Registry
{
    public static class EventRegister
    {
        public static HashSet<string> VisitedMaps = new();

        public static HashSet<string> BossDefeated = new();

        public static HashSet<string> LeftAfterBoss = new();
    }
}
