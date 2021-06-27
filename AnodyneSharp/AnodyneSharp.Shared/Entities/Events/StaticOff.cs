using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    [NamedEntity("Event",null,4)]
    public class StaticOff : Entity
    {
        public StaticOff(EntityPreset preset, Player p) : base(preset.Position,Drawing.DrawOrder.BACKGROUND)
        {
            GlobalState.staticEffect.Deactivate();
            exists = false;
        }
    }
}
