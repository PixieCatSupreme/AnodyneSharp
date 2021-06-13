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

        //Field trade quest
        public bool SpookedMonster = false;

        public Dictionary<string, int> eventKeys = new();

        public void IncEvent(string e)
        {
            if(eventKeys.TryGetValue(e,out int v))
            {
                eventKeys[e] = v + 1;
            }
            else
            {
                eventKeys[e] = 1;
            }
        }

        public int GetEvent(string e)
        {
            if(eventKeys.TryGetValue(e,out int v))
            {
                return v;
            }
            return 0;
        }

        //TODO rock scrawlings
    }
}
