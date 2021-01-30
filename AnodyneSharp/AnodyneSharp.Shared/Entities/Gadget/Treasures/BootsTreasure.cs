using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class BootsTreasure : Treasure
    {
        public BootsTreasure(Vector2 pos) 
            : base("item_jump_shoes", pos, 0, 4)
        {
        }

        public override void GetTreasure()
        {
            base.GetTreasure();

            GlobalState.inventory.CanJump = true;
        }
    }
}
