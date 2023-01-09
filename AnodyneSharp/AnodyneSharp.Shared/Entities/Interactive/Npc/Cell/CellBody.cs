using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Cell
{
    [NamedEntity("NPC", type: "Cell_Body")]
    class CellBody : Entity
    {
        public CellBody(EntityPreset preset, Player p) 
            : base(preset.Position, new StaticSpriteRenderer("cell_bodies", 16, 16, preset.Frame), Drawing.DrawOrder.ENTITIES)
        {
        }
    }
}
