using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", map: "SPACE", 7), Collision(typeof(Player))]
    class SpaceQuest : Entity, Interactable
    {
        bool _played_quest = false;

        public SpaceQuest(EntityPreset preset, Player p)
            : base(preset.Position, new AnimatedSpriteRenderer("space_npcs", 16, 16, new Anim("a",new int[] { 0, 10, 1, 11 },10)), Drawing.DrawOrder.ENTITIES)
        {
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
            if ((quest_progress == 6 || quest_progress == 7) && !_played_quest)
            {
                _played_quest = true;
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_event");
                if (quest_progress == 6) GlobalState.events.IncEvent("GoQuestProgress");
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_normal");
            }
            return true;
        }
    }
}
