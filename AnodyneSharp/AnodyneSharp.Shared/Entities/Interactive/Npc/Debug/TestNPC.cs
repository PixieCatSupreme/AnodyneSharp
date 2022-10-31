using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Debug
{
    [NamedEntity("NPC","npc_test",0), Collision(typeof(Player))]
    public class TestNPC : Entity, Interactable
    {
        public TestNPC(EntityPreset preset, Player p) : base(preset.Position,"npc",16,16,Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("test", "scene_1");
            return true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }
}
