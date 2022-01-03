using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Circus
{
    [NamedEntity("Circus_Folks", "dead", 1)]
    class JavieraDead : Entity
    {
        public JavieraDead(EntityPreset preset, Player p)
            : base(preset.Position, "javiera", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            SetFrame(12);
        }
    }
}