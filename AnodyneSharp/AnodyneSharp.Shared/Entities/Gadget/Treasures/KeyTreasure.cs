using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class KeyTreasure : Treasure
    {
        public KeyTreasure(Vector2 pos)
            : base("key", pos, 0, GlobalState.CURRENT_MAP_NAME == "STREET" ? 2 : -1)
        {
        }

        public override void GetTreasure()
        {
            base.GetTreasure();

            InventoryManager.AddCurrentMapKey();
        }
    }
}
