using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("NPC", "generic", 9)]
    public class DeathPlace : Entity
    {
        Vector2 gridToCheck;

        Player _player;

        public DeathPlace(EntityPreset preset, Player p) 
            : base(preset.Position, DrawOrder.ENTITIES)
        {
            exists = GlobalState.InDeathRoom;
            immovable = true;
            visible = false;

            gridToCheck = MapUtilities.GetRoomUpperLeftPos( GlobalState.CurrentMapGrid + new Point(0,1));

            _player = p;
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            if (_player.Position.Y >= gridToCheck.Y-14)
            {
                GlobalState.ToTitle = true;
                exists = false;
            }
            
        }
    }
}
