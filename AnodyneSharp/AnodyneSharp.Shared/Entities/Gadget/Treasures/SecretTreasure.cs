using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class SecretTreasure : Treasure
    {
        private int _secretID;

        public SecretTreasure(Vector2 pos, int frame, int dialogueID)
            : base("secret_trophies", pos, frame, dialogueID)
        {
            _secretID = frame;
        }

        public override void GetTreasure()
        {
            base.GetTreasure();

            InventoryState.SecretStatus[_secretID] = true;

            DebugLogger.AddInfo($"Got secret {_secretID}");
        }
    }
}