using AnodyneSharp.Dialogue.Tree;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AnodyneSharp.Dialogue
{
    public class DialogueScene
    {
        [JsonIgnore]
        public bool AlignTop { get; private set; }
        [JsonIgnore]
        public int? LoopID { get; private set; }

        [JsonInclude]
        public DialogueState state;

        private List<string> _lines;

        public int Length => _lines.Count;

        public DialogueScene() { }

        public DialogueScene(bool alignTop, int? loopID, List<string> lines)
        {
            AlignTop = alignTop;
            LoopID = loopID;
            _lines = lines;

            state = new DialogueState();
        }

        public string GetDialogue(int id = -1)
        {
            state.dirty = true;

            if (id == -1)
            {
                id = state.line;
            }

            if (id >= _lines.Count)
            {
                id = LoopID ?? 0;
            }

            string line = _lines[id];

            id++;

            if (id >= _lines.Count)
            {
                    state.finished = true;
            }

            state.line = id;

            return line;
        }
    }
}
