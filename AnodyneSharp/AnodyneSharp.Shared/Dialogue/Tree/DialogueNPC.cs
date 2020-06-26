using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Dialogue
{
    public class DialogueNPC
    {
        public bool doesReset;

        Dictionary<string, DialogueArea> _areas;

        public DialogueNPC()
        {
            _areas = new Dictionary<string, DialogueArea>();
            doesReset = false;
        }

        public DialogueArea GetArea(string name)
        {
            if (!_areas.ContainsKey(name))
            {
                return null;
            }

            return _areas[name];
        }

        public void AddArea(string name)
        {
            _areas.Add(name, new DialogueArea());
        }
    }
}
