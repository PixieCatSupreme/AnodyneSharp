using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Registry;

namespace AnodyneSharp.Entities.Events
{
    [NamedEntity("Event", null, 8)]
    public class BossRushKeyReset : Entity
    {
        public BossRushKeyReset(EntityPreset preset, Player p) 
            : base(Microsoft.Xna.Framework.Vector2.Zero)
        {
            GlobalState.inventory.SetMapKeys("BOSSRUSH", 0);

            exists = false;
        }
    }
}
