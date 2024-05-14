using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Archipelago
{
    public class ArchipelagoItem : Item
    {
        public string Reciever { get; set; }
        public string ItemName { get; set; }
        public long LocationID { get; set; }
        public bool IsImportant { get; set; }

        public ArchipelagoItem(int frame, string typeValue, string reciever, string itemName, long locationID, bool isImportant)
            :base(frame, typeValue)
        {
            Reciever = reciever;
            ItemName = itemName;
            LocationID = locationID;
            IsImportant = isImportant;
        }
    }
}
