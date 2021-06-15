using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.GameEvents
{
    public class GameEvent { }

    public class StartScreenTransition : GameEvent { }
    public class EndScreenTransition : GameEvent { }
    public class StartWarp : GameEvent { }
    public class EndWarp : GameEvent { }
}
