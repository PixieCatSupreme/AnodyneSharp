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

            int id = GameConstants.ARCHIPELAGO_START_ID;

            while (!EndOfStream)
            {
                string[] line = SplitNextLine();

                if (int.TryParse(line[0], out int frame))
                {
                    items.Add(id, new(frame, line.Length == 2 ? line[1] : ""));

                    id++;
                }
            }

            return items;
        }
    }
}
