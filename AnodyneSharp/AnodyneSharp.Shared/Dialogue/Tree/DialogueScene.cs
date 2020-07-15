using AnodyneSharp.Dialogue.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Dialogue
{
    public class DialogueScene
    {
        public bool AlignTop { get; private set; }
        public int? LoopID { get; private set; }

        public DialogueState state;

        private List<string> _lines;

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
