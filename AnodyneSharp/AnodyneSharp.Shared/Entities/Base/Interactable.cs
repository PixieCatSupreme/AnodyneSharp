using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
#pragma warning disable IDE1006 // Naming Styles, IInteractable is... ugh
    public interface Interactable
#pragma warning restore IDE1006 // Naming Styles
    {
        bool PlayerInteraction(Facing player_direction);
    }
}
