using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", map: "CELL", 7), Collision(typeof(Player))]
    class CellQuest : Entity, Interactable
    {
        bool _played_quest = false;

        public CellQuest(EntityPreset preset, Player p)
            : base(preset.Position, "cell_bodies", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("idle_d", CreateAnimFrameArray(8));
            AddAnimation("idle_r", CreateAnimFrameArray(9));
            AddAnimation("idle_u", CreateAnimFrameArray(10));
            AddAnimation("idle_l", CreateAnimFrameArray(11));

            Play("idle_d");
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
