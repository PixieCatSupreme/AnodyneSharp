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
            exists = false;

            if (GlobalState.ReturnTarget.map == GlobalState.CURRENT_MAP_NAME) return;
            
            GlobalState.ReturnTarget = new(GlobalState.CURRENT_MAP_NAME, preset.Position - new Vector2(10, 34));
        }
    }
}
