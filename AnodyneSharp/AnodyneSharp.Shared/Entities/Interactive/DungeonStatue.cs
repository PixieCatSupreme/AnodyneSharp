using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive
{
    [NamedEntity("Dungeon_Statue"),Collision(PartOfMap = true)]
    class DungeonStatue : Entity, Interactable
    {
        public DungeonStatue(EntityPreset preset, Player p) : base(preset.Position, "big_statue", 32,48,Drawing.DrawOrder.ENTITIES)
        {
            //TODO: check for windmill activated
            SetFrame(preset.Frame);
            immovable = true;
            width = 30;
            height = 16;
            offset = new Vector2(1, 32);
            Position += offset;
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("dungeon_statue", "one"); //"two" if windmill is open
            return true;
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }
    }
}
