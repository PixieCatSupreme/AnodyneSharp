using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    [NamedEntity("Event",null,3)]
    public class SwapperPolicyEvent : Entity
    {
        public SwapperPolicyEvent(EntityPreset preset, Player p) : base(preset.Position,Drawing.DrawOrder.ENTITIES)
        {
            exists = false;
            switch(preset.TypeValue)
            {
                case "Default":
                    GlobalState.Swapper = GlobalState.SwapperPolicy.Default;
                    break;
                case "Disabled":
                    GlobalState.Swapper = GlobalState.SwapperPolicy.Disallowed;
                    break;
                case "LocalEnabled":
                    GlobalState.Swapper = GlobalState.SwapperPolicy.AllowedOnCurrentScreen;
                    GlobalState.SwapperAllowedCoords = new(Position.ToPoint(), new(160, 160));
                    break;
            }
        }
    }
}
