using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("NPC", "generic", 17, 18), Collision(typeof(Player))]
    class DevEaster : Entity, Interactable
    {
        string scene;

        public DevEaster(EntityPreset preset, Player p) : base(preset.Position, "dev_npcs", 16,16, DrawOrder.ENTITIES)
        {
            immovable = true;
            scene = preset.Frame == 17 ? "melos" : "marina";
            SetFrame(preset.Frame == 17 ? 0 : 10);
        }

        public override void Collided(Entity other)
        {
            Separate(other, this);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("generic_npc", "DEBUG", scene, 0);
            return true;
        }
    }
}
