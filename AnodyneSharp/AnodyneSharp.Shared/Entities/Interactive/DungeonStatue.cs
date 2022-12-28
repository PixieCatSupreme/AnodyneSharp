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
        public static Facing MoveDir(int frame) => frame switch
        {
            0 => Facing.UP,
            _ => Facing.RIGHT
        };

        public DungeonStatue(Vector2 pos, int frame) : base(pos, "big_statue", 32, 48, Drawing.DrawOrder.ENTITIES)
        {
            SetFrame(frame);
            immovable = true;
            width = 30;
            height = 16;
            offset = new Vector2(1, 32);
            Position += offset;
        }

        public DungeonStatue(EntityPreset preset, Player p) : this(preset.Position,preset.Frame)
        {
            if(GlobalState.events.GetEvent("WindmillOpened") != 0)
            {
                Position += FacingDirection(MoveDir(Frame)) * 32;
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if (GlobalState.events.GetEvent("WindmillOpened") != 0)
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("dungeon_statue", "two");
            }
            else
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("dungeon_statue", "one");
            }
            return true;
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }
    }
}
