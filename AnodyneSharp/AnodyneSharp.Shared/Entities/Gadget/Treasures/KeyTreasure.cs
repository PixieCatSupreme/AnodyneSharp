using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class KeyTreasure : Treasure
    {
        private string _mapName;

        public KeyTreasure(Vector2 pos, string mapName)
            : base("key", pos, 0, (GlobalState.events.GetEvent("ReceivedKey") == 0) && string.IsNullOrEmpty(mapName) ? 2 : -1)
        {
            _mapName = mapName;
        }

        public override void GetTreasure()
        {
            base.GetTreasure();

            if (string.IsNullOrEmpty(_mapName))
            {
                GlobalState.events.IncEvent("ReceivedKey");

                GlobalState.inventory.AddCurrentMapKey();
            }
            else
            {
                GlobalState.inventory.AddMapKey(_mapName, 1);

                if (_mapName != GlobalState.CURRENT_MAP_NAME)
                {
                    string name = _mapName[0] + _mapName[1..].ToLower();

                    GlobalState.Dialogue = string.Format(Dialogue.DialogueManager.GetDialogue("misc", "any", "treasure", 10), name);
                }
            }
        }
    }
}
