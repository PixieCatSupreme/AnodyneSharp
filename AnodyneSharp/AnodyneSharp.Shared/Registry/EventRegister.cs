using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Registry
{
    public class EventRegister
    {
        public HashSet<string> VisitedMaps = new();

        public HashSet<string> ActivatedNexusPortals = new();

        public HashSet<string> BossDefeated = new();

        public HashSet<string> LeftAfterBoss = new();
    }
}
