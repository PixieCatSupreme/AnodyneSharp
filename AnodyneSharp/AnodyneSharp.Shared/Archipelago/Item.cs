using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Archipelago
{
    public struct Item
    {
        public int Frame { get; private set; }
        public string TypeValue { get; private set; }

        public Item(int frame, string typeValue)
        {
            Frame = frame;
            TypeValue = typeValue;
        }

    }
}
