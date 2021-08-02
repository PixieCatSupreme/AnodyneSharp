using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", map: "CLIFF", 7), Collision(typeof(Player))]
    public class CliffQuest : Entity, Interactable
    {
        bool played_quest = false;

        public CliffQuest(EntityPreset preset, Player p)
                        : base(preset.Position, "cliffs_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("a", CreateAnimFrameArray(0), 2, true);
            AddAnimation("move", CreateAnimFrameArray(1, 3, 1, 5, 1, 1, 1, 0, 2, 0, 4, 0, 0, 1), 3, false);
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
            if ((quest_progress == 0 || quest_progress == 1) && !played_quest)
            {
                played_quest = true;
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_event");

                if (quest_progress == 0)
                {
                    GlobalState.events.IncEvent("GoQuestProgress");
                }
            }
            else
            {
                Play("move");
                if (QuestNPCHelper.NeedsSecond("cliff", "quest_normal", "second"))
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
