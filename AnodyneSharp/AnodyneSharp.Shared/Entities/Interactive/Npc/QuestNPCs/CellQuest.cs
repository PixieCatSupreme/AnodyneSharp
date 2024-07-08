using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", map: "CELL", 7), Collision(typeof(Player))]
    public class CellQuest : Entity, Interactable
    {
        bool _played_quest = false;

        public static AnimatedSpriteRenderer GetSprite() => new("cell_bodies", 16, 16,
            new Anim("idle_d", new int[] { 8 }, 1),
            new Anim("idle_r", new int[] { 9 }, 1),
            new Anim("idle_u", new int[] { 10 }, 1),
            new Anim("idle_l", new int[] { 11 }, 1)
            );

        public CellQuest(EntityPreset preset, Player p)
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
            facing = FlipFacing(player_direction);

            PlayFacing("idle");

            int quest_progress = GlobalState.events.GetEvent("GoQuestProgress");
            if ((quest_progress == 4 || quest_progress == 5) && !_played_quest)
            {
                _played_quest = true;
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_event");
                if (quest_progress == 4) GlobalState.events.IncEvent("GoQuestProgress");
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_normal");
            }
            return true;
        }
    }
}
