using AnodyneSharp.Archipelago;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class ArchipelagoTreasure : Treasure
    {
        private ArchipelagoItem _item;

        public ArchipelagoTreasure(Vector2 pos, ArchipelagoItem item)
            : base("archipelago_items", pos, item.IsImportant ? 0 : 1, -3)
        {
            _item = item;
        }

        public override void GetTreasure()
        {
            SoundManager.PlaySoundEffect("gettreasure");

            Flicker(1);
            exists = true;

            GlobalState.Dialogue = string.Format(Dialogue.DialogueManager.GetDialogue("misc", "any", "treasure", 9), _item.Reciever, _item.ItemName);

            //TODO item call to client
        }
    }
}