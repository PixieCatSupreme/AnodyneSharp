using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "6")]
    public class OneWayDoor : Entity
    {
        //This is just the target for a one way door. It's supposed to do literally nothing!
        public OneWayDoor(EntityPreset preset, Player player)
            : base(Vector2.Zero, 0)
        {
            exists = false;
        }
    }
}
