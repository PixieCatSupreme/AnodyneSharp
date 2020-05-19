using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Input
{
    public enum KeyFunctions
    {
        Up = 1,
        Down,
        Left,
        Right,
        Accept,
        Cancel,
        Pause,
        ToggleFPS,
        Screenshot
    }

#if DEBUG
    public enum DebugKeyFunctions
    {
        ShowState = 1000,
        HardReset,
        SmallResolution,
        DefaultResolution,
        PreviousSong,
        NextSong,
        StopSong
    }
#endif

    /// <summary>
    /// An easy handler to keep track of pressed keys and mouse buttons and has some methods to check if the are pressed or not.
    /// </summary>
    public static class KeyInput
    {
        private enum ButtonState
        {
            NONE,
            HELD,
            PRESSED
        };

        private static ButtonState UpdateButton(ButtonState current, bool pressed)
        {
            if(!pressed)
            {
                return ButtonState.NONE;
            }
            if(current == ButtonState.NONE)
            {
                return ButtonState.PRESSED;
            }
            return ButtonState.HELD;
        }

        public static Dictionary<Enum,RebindableKey> RebindableKeys;

        private static Dictionary<Keys, ButtonState> KeyState;
        private static Dictionary<Buttons, ButtonState> ControllerState;

        static KeyInput()
        {
            RebindableKeys = new Dictionary<Enum, RebindableKey>();

            KeyState = Enumerable.ToDictionary(Enum.GetValues(typeof(Keys)) as IEnumerable<Keys>, k => k, k => ButtonState.NONE);
        }

        /// <summary>
        /// Updates the internal key states.
        /// </summary>
        public static void Update()
        {
            KeyboardState s = Keyboard.GetState();
            
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                KeyState[key] = UpdateButton(KeyState[key], s.IsKeyDown(key));
            }
        }

        private static ButtonState GetRebindableKeyState(Enum name)
        {
            ButtonState state = ButtonState.NONE;
            if (RebindableKeys.TryGetValue(name, out RebindableKey key))
            {
                state = Enumerable.Max(key.Keys, k => KeyState[k]);
            }
            return state;
        }

        /// <summary>
        /// Simply checks if a rebindable key is down based on its function name.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static bool IsRebindableKeyPressed(Enum functionName)
        {
            return GetRebindableKeyState(functionName) != ButtonState.NONE;
        }

        /// <summary>
        /// Returns if a rebindable key was just pressed this frame.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static bool JustPressedRebindableKey(Enum functionName)
        {
            return GetRebindableKeyState(functionName) == ButtonState.PRESSED;
        }

        /// <summary>
        /// Simply checks if a key is down.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyPressed(Keys key)
        {
            return KeyState[key] != ButtonState.NONE;
        }

        /// <summary>
        /// Simply checks if any of the keys in the list is down.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool IsAnyKeyPressed(List<Keys> keys)
        {
            foreach (var key in keys)
            {
                if (IsKeyPressed(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns if a key was just pressed this frame.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool JustPressedKey(Keys key)
        {
            return KeyState[key] == ButtonState.PRESSED;
        }

        /// <summary>
        /// Compares two key function names.
        /// </summary>
        /// <param name="functionName1"></param>
        /// <param name="functionName2"></param>
        /// <returns></returns>
        private static bool CompareFunctionNames(Enum functionName1, Enum functionName2)
        {
            KeyFunctions function1 = (KeyFunctions)functionName1;
            KeyFunctions function2 = (KeyFunctions)functionName2;

            return function1 == function2;
        }
    }
}
