using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("NPC", "generic", 11), Collision(PartOfMap = true)]
    public class GoHappyBlocker : Entity
    {
        public GoHappyBlocker(EntityPreset preset, Player p)
            : base(preset.Position, new AnimatedSpriteRenderer( "briar_ground_thorn", 16, 16, new Anim("a", new int[] { 9, 10 }, 8)), DrawOrder.ENTITIES)
        {
            if (GlobalState.events.GetEvent("BlueDone") == 1)
            {
                exists = false;
                return;
            }

            immovable = true;

            width = 20;
            height = 20;

            Position.Y += 2;
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }
    }
}
