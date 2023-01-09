using AnodyneSharp.Entities.Base.Rendering;
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
        public static AnimatedSpriteRenderer GetSprite(bool cell) => new("whiteportal", 16, 16,
            new Anim("a", cell ? new int[] { 4, 5 } : new int[] { 0, 1, 2 },8)
            );

        public BlankPortal(EntityPreset preset, Player player) 
            : base(preset, player, GetSprite(GlobalState.IsCell), "teleport_up")
        {
            width = height = 2;
            CenterOffset();

            if (GlobalState.IsCell)
            {
                teleportOffset = new Vector2(0, -12);
            }
            Play("a");
        }
    }
}
