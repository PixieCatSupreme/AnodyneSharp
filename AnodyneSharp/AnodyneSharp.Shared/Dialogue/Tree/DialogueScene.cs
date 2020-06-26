using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Dialogue
{
    public class DialogueScene
    {
        public bool AlignTop { get; private set; }
        public int? LoopID { get; private set; }

        private List<string> _lines;

        public DialogueScene(bool alignTop, int? loopID, List<string> lines)
        {
            AlignTop = alignTop;
            LoopID = loopID;
            _lines = lines;
        }

        public string GetDialogue(int id)
        {
            if (id >= _lines.Count)
            {
                id = LoopID != null ? LoopID.Value : 0;
            }

            return _lines[id];
        }
    }
}
