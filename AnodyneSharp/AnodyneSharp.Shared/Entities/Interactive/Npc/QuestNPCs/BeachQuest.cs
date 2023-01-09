using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
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

        public static AnimatedSpriteRenderer GetSprite() => new("beach_npcs", 16, 16,
            new Anim("a", new int[] { 0 },1),
            new Anim("turn", new int[] { 1 },1)
            );

        public BeachQuest(EntityPreset preset, Player p) 
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
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
                if (QuestNPCHelper.NeedsSecond("beach", "quest_normal", "second"))
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "second");
                }
                else
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_normal");
                }
            }

            Play("turn");

            return true;
        }
    }
}
