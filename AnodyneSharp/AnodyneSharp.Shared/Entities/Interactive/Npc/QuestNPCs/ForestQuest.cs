using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", map: "FOREST", 7), Collision(typeof(Player))]
    public class ForestQuest : Entity, Interactable
    {
        bool _played_quest = false;

        public ForestQuest(EntityPreset preset, Player p) 
            : base(preset.Position, "forest_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("a", CreateAnimFrameArray(10, 10, 10, 11, 10, 10, 10, 12), 3);

            Play("a");
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            int quest_progress = GlobalState.events.GetEvent("GoQuestProgress");
            if ((quest_progress == 2 || quest_progress == 3) && !_played_quest)
            {
                _played_quest = true;
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_event");

                if (quest_progress == 2)
                {
                    GlobalState.events.IncEvent("GoQuestProgress");
                }
            }
            else
            {
                if (QuestNPCHelper.NeedsSecond("forest", "quest_normal", "second"))
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "second");
                }
                else
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_normal");
                }
            }

            return true;
        }
    }
}
