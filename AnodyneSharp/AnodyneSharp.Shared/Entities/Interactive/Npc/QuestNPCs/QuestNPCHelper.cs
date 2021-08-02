using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    static public class QuestNPCHelper
    {
        public static bool NeedsSecond(string name, string normal, string second)
        {
            string eventName = $"{name}-QuestCounter";
            int previousCount = GlobalState.events.GetEvent(eventName);
            int currentCount = GlobalState.events.BossDefeated.Count;
            if(DialogueManager.IsSceneDirty("generic_npc",normal))
            {
                bool seenSecond = DialogueManager.IsSceneDirty("generic_npc", second);
                bool finishedFirst = DialogueManager.IsSceneFinished("generic_npc", normal);

                if (finishedFirst && (currentCount >= 6 || currentCount > previousCount || seenSecond))
                {
                    GlobalState.events.SetEvent(eventName, currentCount);
                    return true;
                }
                return false;
            }
            else
            {
                //First time talking never do second
                GlobalState.events.SetEvent(eventName, currentCount);
                return false;
            }
        }
    }
}
