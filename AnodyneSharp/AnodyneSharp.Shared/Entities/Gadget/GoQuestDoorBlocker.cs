using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity]
    public class GoQuestDoorBlocker : Entity
    {
        Events.DoorToggle doorBlocker;

        public GoQuestDoorBlocker(EntityPreset preset, Player p) : base(preset.Position)
        {
            visible = false;
            doorBlocker = new(preset.Position, 16, 16);
            exists = false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            if (GlobalState.events.GetEvent("GoQuestProgress") < 7)
            {
                return new List<Entity>() { doorBlocker };
            }
            return base.SubEntities();
        }
    }
}
