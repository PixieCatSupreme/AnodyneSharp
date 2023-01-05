using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.QuestNPCs
{
    [NamedEntity("NPC", type: "generic", map: "FIELDS", 7), Collision(typeof(Player))]
    class FieldsQuest : Entity, Interactable
    {
        Bush _bush;
        bool _played_quest = false;

        public FieldsQuest(EntityPreset preset, Player p) 
            : base(preset.Position, new AnimatedSpriteRenderer("fields_npcs", 32, 32, new Anim("a", new int[] { 15, 15, 15, 15, 15, 15, 15, 15, 16, 17, 17, 18, 18 }, 18, true)), Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
            width = height = 16;
            Position.Y += 16;
            offset.Y = 16;
            _bush = new(Position + Vector2.UnitX * 16);
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(_bush, 1);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            int quest_progress = GlobalState.events.GetEvent("GoQuestProgress");
            if ((quest_progress == 3 || quest_progress == 4) && !_played_quest)
            {
                _played_quest = true;
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_event");
                if (quest_progress == 3) GlobalState.events.IncEvent("GoQuestProgress");
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "quest_normal");
            }
            return true;
        }

        [Collision(typeof(Player), typeof(Broom))]
        class Bush : Entity
        {
            public Bush(Vector2 pos) : base(pos, "fields_npcs", 16, 16, Drawing.DrawOrder.BG_ENTITIES)
            {
                SetFrame(30);
                immovable = true;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                if (other is Player p)
                {
                    Separate(this, p);
                }
                else
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "bush");
                }
            }
        }
    }
}
