using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.GameEvents
{
    public record GameEvent { }

    public record StartScreenTransition : GameEvent { }
    public record EndScreenTransition : GameEvent { }
    public record StartWarp : GameEvent { }
    public record EndWarp : GameEvent { }

    public record BroomUsed : GameEvent { }

    public record ChangeCardCount(int Count) : GameEvent { }
}
