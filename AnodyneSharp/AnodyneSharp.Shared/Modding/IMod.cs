using AnodyneSharp.States.MenuSubstates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnodyneSharp.Modding
{
    public interface IMod
    {
        void ChangeMainMenu(ref List<(string name, Func<Substate> create)> menuEntries) { }

        void ChangePauseMenu(ref List<(string name, Func<Substate> create)> menuEntries) { }

        Stream OnManifestLoad(Stream stream, string path) {
            return stream;
        }

        void Update() { }
    }
}
