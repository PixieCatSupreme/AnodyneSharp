using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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

            int secretLength = GlobalState.inventory.SecretStatus.Length;
            if (_secretID >= secretLength)
            {
                GlobalState.inventory.SecretStatus = GlobalState.inventory.SecretStatus.Concat(Enumerable.Repeat(false, _secretID - secretLength + 1)).ToArray();
            }

            GlobalState.inventory.SecretStatus[_secretID] = true;

            DebugLogger.AddInfo($"Got secret {_secretID}");
        }
    }
}