using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "4"), Collision(typeof(Player))]
    public class BlankPortal : Door
    {
        public BlankPortal(EntityPreset preset, Player player) 
            : base(preset, player, "whiteportal", 16, 16, "teleport_up")
        {
            width = height = 2;
            CenterOffset();

            if (GlobalState.CURRENT_MAP_NAME == "CELL")
            {
                AddAnimation("a", CreateAnimFrameArray(4,5),8,true);
                teleportOffset = new Vector2(0, -12);
            }
            else
            {
                AddAnimation("a", CreateAnimFrameArray(0,1,2), 8, true);
            }
            Play("a");
        }
    }
}
