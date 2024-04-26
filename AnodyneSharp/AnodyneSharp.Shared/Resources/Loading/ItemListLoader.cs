using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Archipelago;
using AnodyneSharp.Registry;

namespace AnodyneSharp.Resources.Loading
{
    public class ItemListLoader : ContentLoader
    {
        protected internal ItemListLoader(string filePath)
            : base(filePath)
        {
        }

        public Dictionary<long, Item> LoadItems()
        {
            Dictionary<long, Item> items = new();

            while (!EndOfStream)
            {
                string[] line = SplitNextLine();

                if (line.Length == 3 &&
                    long.TryParse(line[0], out long itemID) &&
                    int.TryParse(line[1], out int frame))
                {
                    items.Add(itemID, new(frame, line[2]));
                }
            }

            return items;
        }
    }
}
