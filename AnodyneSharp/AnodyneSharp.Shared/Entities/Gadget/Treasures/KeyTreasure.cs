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
            : base("key", pos, 0, -1)
        {
            _mapName = mapName;
        }

        public override void GetTreasure()
        {
            SoundManager.PlaySoundEffect("gettreasure");

            Flicker(1);
            exists = true;

            GlobalState.events.IncEvent("ReceivedKey");

            if (string.IsNullOrEmpty(_mapName))
            {
                GlobalState.inventory.AddCurrentMapKey();
            }
            else
            {
                GlobalState.inventory.AddMapKey(_mapName, 1);

                string name = _mapName[0] + _mapName[1..].ToLower();

                GlobalState.Dialogue = string.Format(Dialogue.DialogueManager.GetDialogue("misc", "any", "treasure", 10), name);
            }
        }
    }
}
