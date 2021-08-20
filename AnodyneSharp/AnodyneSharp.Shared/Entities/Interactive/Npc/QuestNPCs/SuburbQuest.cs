using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", map: "SUBURB", 7), Collision(typeof(Player))]
    class SuburbQuest : Entity, Interactable
    {
        bool _played_quest = false;

        public SuburbQuest(EntityPreset preset, Player p)
            : base(preset.Position, "suburb_walkers", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("a", CreateAnimFrameArray(0, 1), 4, true);

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
            if ((quest_progress == 5 || quest_progress == 6) && !_played_quest)
            {
                _played_quest = true;
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_event");
                if (quest_progress == 5) GlobalState.events.IncEvent("GoQuestProgress");
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_normal");
            }
            return true;
        }
    }
}
