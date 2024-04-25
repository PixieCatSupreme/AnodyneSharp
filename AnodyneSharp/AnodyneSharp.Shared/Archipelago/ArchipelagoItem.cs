using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Archipelago
{
    public struct ArchipelagoItem
    {
        public string Reciever { get; set; }
        public string ItemName { get; set; }
        public bool IsImportant { get; set; }

        public ArchipelagoItem(string reciever, string itemName, bool isImportant)
        {
            Reciever = reciever;
            ItemName = itemName;
            IsImportant = isImportant;
        }
    }
}
