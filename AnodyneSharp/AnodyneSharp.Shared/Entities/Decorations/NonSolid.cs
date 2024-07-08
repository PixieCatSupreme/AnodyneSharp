using AnodyneSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Decorations
{
    [NamedEntity("Nonsolid", "Rail_CROWD")]
    public class CrowdRail : Entity
    {
        public CrowdRail(EntityPreset preset, Player p) : base(preset.Position, "rail_CROWD", 16, 22, DrawOrder.ENTITIES) { }
    }

    [NamedEntity("Nonsolid","Rail_1")]
    public class Rail : Entity
    {
        public Rail(EntityPreset preset, Player p) : base(preset.Position, "rail", 16, 16, DrawOrder.ENTITIES) { }
    }

}
