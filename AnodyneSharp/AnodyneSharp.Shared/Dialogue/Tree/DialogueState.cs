using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AnodyneSharp.Dialogue.Tree
{
    public class DialogueState
    {
        [JsonInclude]
        public bool dirty = false;
        [JsonInclude]
        public bool finished = false;
        [JsonInclude]
        public int line = 0;
    }
}
