using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    [NamedEntity("Event", null, 5)]
    public class DungeonEntrance : Entity
    {
        public DungeonEntrance(EntityPreset preset, Player p) : base(preset.Position, DrawOrder.ENTITIES)
        {
            EntityPreset moved = new EntityPreset(preset.Type, preset.Position - new Vector2(10, 34), preset.EntityID, preset.Frame);
            GlobalState.ReturnTarget = new DoorMapPair(moved, GlobalState.CURRENT_MAP_NAME);
            exists = false;
        }
    }
}
