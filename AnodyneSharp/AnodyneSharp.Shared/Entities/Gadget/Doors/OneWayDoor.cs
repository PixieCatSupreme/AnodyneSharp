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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Both parameters are required for the entity preset system to work.")]
        public OneWayDoor(EntityPreset preset, Player player)
            : base(Vector2.Zero, 0)
        {
            exists = false;
        }
    }
}
