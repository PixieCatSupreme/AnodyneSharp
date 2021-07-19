using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", map: "BEACH", 7), Collision(typeof(Player))]
    public class BeachQuest : Entity, Interactable
    {
        bool _played_quest = false;

        public BeachQuest(EntityPreset preset, Player p) 
            : base(preset.Position, "beach_npcs", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("a", CreateAnimFrameArray(0), 2, true);
            AddAnimation("turn", CreateAnimFrameArray(1), 2, true);
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
            if ((quest_progress == 1 || quest_progress == 2) && !_played_quest)
            {
                _played_quest = true;
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_event");

                if (quest_progress == 1)
                {
                    GlobalState.events.IncEvent("GoQuestProgress");
                }
            }
            else
            {
                //TODO after boss 2nd talk
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_normal");
            }

            Play("turn");

            return true;
        }
    }
}
