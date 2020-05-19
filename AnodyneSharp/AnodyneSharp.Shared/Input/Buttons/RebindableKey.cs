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

        public List<Keys> Keys;
        public List<Buttons> Buttons;
        public List<MouseButton> MouseButtons;
        public List<GestureType> TouchGestures;

        public PlayerIndex? GamePadPlayerIndex;

        public RebindableKey(Keys key)
        {
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>();
            MouseButtons = new List<MouseButton>();
            TouchGestures = new List<GestureType>();

            GamePadPlayerIndex = null;
        }

        public RebindableKey(Buttons button)
        {
            Keys = new List<Keys>();
            Buttons = new List<Buttons>() { button };
            MouseButtons = new List<MouseButton>();
            TouchGestures = new List<GestureType>();

            GamePadPlayerIndex = PlayerIndex.One;
        }

        public RebindableKey(Buttons button, PlayerIndex playerIndex)
        {
            Keys = new List<Keys>();
            Buttons = new List<Buttons>() { button };
            MouseButtons = new List<MouseButton>();
            TouchGestures = new List<GestureType>();

            GamePadPlayerIndex = playerIndex;
        }

        public RebindableKey(MouseButton mouseButton)
        {
            Keys = new List<Keys>();
            Buttons = new List<Buttons>();
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>() { mouseButton };
        }

        public RebindableKey(Keys key, Buttons button)
        {
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>() { button };
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(Keys key, Buttons button, PlayerIndex playerIndex)
        {
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>() { button };
            GamePadPlayerIndex = playerIndex;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(Keys key, Buttons button, MouseButton mouseButton)
        {
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>() { button };
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>() { mouseButton };
        }

        public RebindableKey(Keys key, Buttons button, PlayerIndex playerIndex, MouseButton mouseButton)
        {
            Keys = new List<Keys>() { key };
            Buttons = new List<Buttons>() { button };
            GamePadPlayerIndex = playerIndex;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>() { mouseButton };
        }
        public RebindableKey(List<Keys> keys)
        {
            Keys = keys;
            Buttons = new List<Buttons>();
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }
        public RebindableKey(List<Buttons> buttons)
        {
            Keys = new List<Keys>();
            Buttons = buttons;
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(List<MouseButton> mouseButtons)
        {
            Keys = new List<Keys>();
            Buttons = new List<Buttons>();
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = mouseButtons;
        }

        public RebindableKey(List<MouseButton> mouseButtons, List<GestureType> gestureTypes)
        {
            Keys = new List<Keys>();
            Buttons = new List<Buttons>();
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = gestureTypes;

            MouseButtons = mouseButtons;
        }

        public RebindableKey(List<Keys> keys, List<Buttons> buttons)
        {
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(List<Keys> keys, List<Buttons> buttons, List<MouseButton> mouseButtons)
        {
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = new List<GestureType>();

            MouseButtons = mouseButtons;
        }


        public RebindableKey(List<Keys> keys, List<Buttons> buttons, List<MouseButton> mouseButtons, List<GestureType> gestureTypes)
        {
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = PlayerIndex.One;
            TouchGestures = gestureTypes;

            MouseButtons = mouseButtons;
        }

        public RebindableKey(List<Keys> keys, List<Buttons> buttons, PlayerIndex playerIndex)
        {
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = playerIndex;
            TouchGestures = new List<GestureType>();

            MouseButtons = new List<MouseButton>();
        }

        public RebindableKey(List<Keys> keys, List<Buttons> buttons, List<MouseButton> mouseButtons, PlayerIndex playerIndex)
        {
            Keys = keys;
            Buttons = buttons;
            GamePadPlayerIndex = playerIndex;
            TouchGestures = new List<GestureType>();

            MouseButtons = mouseButtons;
        }
    }
}
