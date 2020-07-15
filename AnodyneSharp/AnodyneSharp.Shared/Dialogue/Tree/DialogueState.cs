using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Dialogue.Tree
{
    public class DialogueState
    {
        public bool dirty;
        public bool finished;
        public int line;

        public DialogueState()
        {
            dirty = false;
            finished = false;
            line = 0;
        }
    }
}
