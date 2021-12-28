using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Blank
{
    [NamedEntity("Console", null, "BLANK", 1), Collision(typeof(Player))]
    class BlankConsole : Entity, Interactable
    {
        public BlankConsole(EntityPreset preset, Player p)
            : base(preset.Position, "console", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("flicker", CreateAnimFrameArray(0, 1, 2), 10);

            Play("flicker");

            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("rock", "NEXUS", "five");

            return true;
        }
    }
}