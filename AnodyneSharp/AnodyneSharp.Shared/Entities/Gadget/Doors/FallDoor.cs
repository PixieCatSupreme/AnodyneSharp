using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "8"), Collision(typeof(Player))]
    public class FallDoor : Door
    {
        public FallDoor(EntityPreset preset, Player player)
            : base(preset, player)
        {
            visible = false;
        }

        protected override void TeleportPlayer()
        {
            //TODO: Do falling teleport event
            base.TeleportPlayer();
        }
    }
}
