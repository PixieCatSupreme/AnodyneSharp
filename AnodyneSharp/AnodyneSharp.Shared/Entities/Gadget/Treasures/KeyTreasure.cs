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
            : base("key", pos, 0, (GlobalState.events.GetEvent("ReceivedKey") == 0) ? 2 : -1)
        {
        }

        public override void GetTreasure()
        {
            base.GetTreasure();
            GlobalState.events.IncEvent("ReceivedKey");
            GlobalState.inventory.AddCurrentMapKey();
        }
    }
}
