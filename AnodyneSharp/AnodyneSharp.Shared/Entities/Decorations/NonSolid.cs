using AnodyneSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Decorations
{
    [NamedEntity("Nonsolid", "Rail_CROWD")]
    class CrowdRail : Entity
    {
        public CrowdRail(EntityPreset preset, Player p) : base(preset.Position, "rail_CROWD", 16, 22, Drawing.DrawOrder.FG_SPRITES) { }
    }

    [NamedEntity("Nonsolid","Rail_1")]
    class Rail : Entity
    {
        public Rail(EntityPreset preset, Player p) : base(preset.Position, "rail", 16, 16, DrawOrder.FG_SPRITES) { }
    }

}
