using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace AnodyneSharp.Input
{
    public struct RebindableKey
    {
        public PlayerIndex PlayerIndexOrDefault
        {
            get
            {
                return GamePadPlayerIndex ?? PlayerIndex.One;
            }
        }

        public Enum FunctionName { get; private set; }
        public List<Keys> Keys;
        public List<Buttons> Buttons;
        public List<MouseButton> MouseButtons;
        public List<GestureType> TouchGestures;

        public PlayerIndex? GamePadPlayerIndex;

        public RebindableKey(Enum function, Keys key)
        {
            FunctionName = function;
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>();
            MouseButtons = new List<MouseButton>();
            TouchGestures = new List<GestureType>();

            GamePadPlayerIndex = null;
        }

        public RebindableKey(Enum function, Buttons button)
        {
            FunctionName = function;
            Keys = new List<Keys>();
            Buttons = new List<Buttons>() { button };
            MouseButtons = new List<MouseButton>();
            TouchGestures = new List<GestureType>();

            GamePadPlayerIndex = PlayerIndex.One;
        }

        public RebindableKey(Enum function, Buttons button, PlayerIndex playerIndex)
        {
            FunctionName = function;
            Keys = new List<Keys>();
            Buttons = new List<Buttons>() { button };
            MouseButtons = new List<MouseButton>();
            TouchGestures = new List<GestureType>();

            GamePadPlayerIndex = playerIndex;
        }

        public RebindableKey(Enum function, MouseButton mouseButton)
        {
            FunctionName = function;
            Keys = new List<Keys>();
            Buttons = new List<Buttons>();
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>() { mouseButton };
        }

        public RebindableKey(Enum function, Keys key, Buttons button)
        {
            FunctionName = function;
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>() { button };
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(Enum function, Keys key, Buttons button, PlayerIndex playerIndex)
        {
            FunctionName = function;
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>() { button };
            GamePadPlayerIndex = playerIndex;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(Enum function, Keys key, Buttons button, MouseButton mouseButton)
        {
            FunctionName = function;
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>() { button };
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>() { mouseButton };
        }

        public RebindableKey(Enum function, Keys key, Buttons button, PlayerIndex playerIndex, MouseButton mouseButton)
        {
            FunctionName = function;
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>() { button };
            GamePadPlayerIndex = playerIndex;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>() { mouseButton };
        }
        public RebindableKey(Enum function, List<Keys> keys)
        {
            FunctionName = function;
            Keys = keys;
            Buttons = new List<Buttons>();
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }
        public RebindableKey(Enum function, List<Buttons> buttons)
        {
            FunctionName = function;
            Keys = new List<Keys>();
            Buttons = buttons;
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(Enum function, List<MouseButton> mouseButtons)
        {
            FunctionName = function;
            Keys = new List<Keys>();
            Buttons = new List<Buttons>();
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = mouseButtons;
        }

        public RebindableKey(Enum function, List<MouseButton> mouseButtons, List<GestureType> gestureTypes)
        {
            FunctionName = function;
            Keys = new List<Keys>();
            Buttons = new List<Buttons>();
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = gestureTypes;

            MouseButtons = mouseButtons;
        }

        public RebindableKey(Enum function, List<Keys> keys, List<Buttons> buttons)
        {
            FunctionName = function;
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(Enum function, List<Keys> keys, List<Buttons> buttons, List<MouseButton> mouseButtons)
        {
            FunctionName = function;
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = mouseButtons;
        }


        public RebindableKey(Enum function, List<Keys> keys, List<Buttons> buttons, List<MouseButton> mouseButtons, List<GestureType> gestureTypes)
        {
            FunctionName = function;
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = gestureTypes;

            MouseButtons = mouseButtons;
        }

        public RebindableKey(Enum function, List<Keys> keys, List<Buttons> buttons, PlayerIndex playerIndex)
        {
            FunctionName = function;
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = playerIndex;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(Enum function, List<Keys> keys, List<Buttons> buttons, List<MouseButton> mouseButtons, PlayerIndex playerIndex)
        {
            FunctionName = function;
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = playerIndex;
            TouchGestures = new List<GestureType>();

            MouseButtons = mouseButtons;
        }
    }
}
