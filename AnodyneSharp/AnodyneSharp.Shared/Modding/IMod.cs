using AnodyneSharp.States.MenuSubstates;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Modding
{
    public interface IMod
    {
        void ChangeMainMenu(ref List<(string name, Func<Substate> create)> menuEntries) { }
    }
}
