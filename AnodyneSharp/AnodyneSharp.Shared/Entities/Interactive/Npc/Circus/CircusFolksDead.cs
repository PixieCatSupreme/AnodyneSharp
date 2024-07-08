using AnodyneSharp.Entities.Base.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Circus
{
    [NamedEntity("Circus_Folks", "dead", 0)]
    public class ArthurDead : Entity
    {
        public ArthurDead(EntityPreset preset, Player p)
            : base(preset.Position, new StaticSpriteRenderer("arthur", 16, 16, 18), Drawing.DrawOrder.ENTITIES)
        {
        }
    }

    [NamedEntity("Circus_Folks", "dead", 1)]
    public class JavieraDead : Entity
    {
        public JavieraDead(EntityPreset preset, Player p)
            : base(preset.Position, new StaticSpriteRenderer("javiera", 16, 16, 12), Drawing.DrawOrder.ENTITIES)
        {
        }
    }
}
