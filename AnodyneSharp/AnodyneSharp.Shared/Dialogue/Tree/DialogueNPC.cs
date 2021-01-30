using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AnodyneSharp.Dialogue
{
    public class DialogueNPC
    {
        [JsonIgnore]
        public bool doesReset;

        [JsonInclude]
        public Dictionary<string, DialogueArea> _areas;

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
