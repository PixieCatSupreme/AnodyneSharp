using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity, Collision(typeof(Player))]
    public class Sadbro : Entity, Interactable
    {
        EntityPreset preset;

        public Sadbro(EntityPreset preset, Player p)
            : base(preset.Position, "sadman", 16, 16, DrawOrder.ENTITIES)
        {
            immovable = true;
            this.preset = preset;
            AddAnimation("a", CreateAnimFrameArray(0, 1), 2);
            Play("a");
        }

        public override void Collided(Entity other)
        {
            Separate(other, this);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("sadbro", "initial_forced");
            if(preset.Activated)
            {
                if(GlobalState.events.BossDefeated.Contains("BEDROOM"))
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("sadbro", "bedroom_done");
                }
                else if(GlobalState.events.VisitedMaps.Contains("BEDROOM"))
                {
                    GlobalState.Dialogue = DialogueManager.GetDialogue("sadbro", "bedroom_not_done");
                }
            }
            preset.Activated = true;
            return true;
        }
    }
}
