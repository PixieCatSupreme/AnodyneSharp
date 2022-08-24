using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("NPC", "generic", 11), Collision(PartOfMap = true)]
    internal class GoHappyBlocker : Entity
    {
        public GoHappyBlocker(EntityPreset preset, Player p)
            : base(preset.Position, "briar_ground_thorn", 16, 16, DrawOrder.ENTITIES)
        {
            if (GlobalState.events.GetEvent("BriarBlueDone") == 1)
            {
                exists = false;
                return;
            }

            immovable = true;

            width = 20;
            height = 20;

            Position.Y += 2;

            AddAnimation("a", CreateAnimFrameArray(7, 11), 8);
            Play("a");
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }
    }
}
