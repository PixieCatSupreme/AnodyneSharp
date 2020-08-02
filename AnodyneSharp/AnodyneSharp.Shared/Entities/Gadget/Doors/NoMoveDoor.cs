using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "9"), Collision(typeof(Player))]
    public class NoMoveDoor : Door
    {
        public NoMoveDoor(EntityPreset preset, Player player)
            : base(preset, player)
        {
            visible = false;
        }
    }
}
