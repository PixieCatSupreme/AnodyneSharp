using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities
{
    [NamedEntity("Event", "entrance", 5)]
    public class DungeonEntrance : Entity
    {
        public DungeonEntrance(EntityPreset preset, Player p) : base(preset.Position)
        {
            exists = false;

            if (GlobalState.ReturnTarget.map == GlobalState.CURRENT_MAP_NAME) return;
            
            GlobalState.ReturnTarget = new(GlobalState.CURRENT_MAP_NAME, preset.Position - new Vector2(10, 34));
        }
    }

    [NamedEntity("Event", "entrance_ref", 5)]
    public class DungeonEntranceRef : Entity
    {
        public DungeonEntranceRef(EntityPreset preset, Player p) : base(preset.Position)
        {
            exists = false;

            _ = new DungeonEntrance(EntityManager.GetLinkGroup(preset.LinkID).Where(s => s.TypeValue == "entrance").First(),p);
        }
    }
}
