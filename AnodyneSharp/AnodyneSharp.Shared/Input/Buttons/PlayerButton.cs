using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Input
{
    public struct PlayerButton
    {
        public Buttons Button { get; private set; }
        public PlayerIndex CurrentPlayerIndex { get; private set; }

        public PlayerButton(Buttons button, PlayerIndex playerIndex)
        {
            Button = button;
            CurrentPlayerIndex = playerIndex;
        }
    }
}