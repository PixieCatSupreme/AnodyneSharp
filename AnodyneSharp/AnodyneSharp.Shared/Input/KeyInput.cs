using Microsoft.Xna.Framework;
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
        PreviousPage,
        NextPage
    }

    /// <summary>
    /// An easy handler to keep track of pressed keys and mouse buttons and has some methods to check if the are pressed or not.
    /// </summary>
    public static class KeyInput
    {
        private enum InputState
        {
            NONE,
            HELD,
            PRESSED
        };

        private static InputState UpdateInput(InputState current, bool pressed)
        {
            if (!pressed)
            {
                return InputState.NONE;
            }
            if (current == InputState.NONE)
            {
                return InputState.PRESSED;
            }
            return InputState.HELD;
        }

        public static Dictionary<KeyFunctions, RebindableKey> RebindableKeys;

        private static Dictionary<Keys, InputState> KeyState;
        private static Dictionary<Buttons, InputState>[] ControllerState;

        static KeyInput()
        {
            RebindableKeys = new Dictionary<KeyFunctions, RebindableKey>();
            ControllerState = new Dictionary<Buttons, InputState>[4];

            KeyState = Enumerable.ToDictionary(Enum.GetValues(typeof(Keys)) as IEnumerable<Keys>, k => k, k => InputState.NONE);

            for (int i = 0; i < ControllerState.Length; i++)
            {
                ControllerState[i] = Enumerable.ToDictionary(Enum.GetValues(typeof(Buttons)) as IEnumerable<Buttons>, k => k, k => InputState.NONE);
            }
        }

        /// <summary>
        /// Updates the internal key states.
        /// </summary>
        public static void Update()
        {
            KeyboardState s = Keyboard.GetState();

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                KeyState[key] = UpdateInput(KeyState[key], s.IsKeyDown(key));
            }

            for (int i = 0; i < ControllerState.Length; i++)
            {
                GamePadState g = GamePad.GetState(i);
                var dic = ControllerState[i];

                foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
                {
                    dic[button] = UpdateInput(dic[button], g.IsButtonDown(button));
                }
            }
        }

        private static InputState GetRebindableKeyState(KeyFunctions name)
        {
            InputState state = InputState.NONE;
            if (RebindableKeys.TryGetValue(name, out RebindableKey key))
            {
                var states = Enumerable.Select(key.Keys, k => KeyState[k]);
                if (key.GamePadPlayerIndex != null)
                {
                    states = states.Concat(Enumerable.Select(key.Buttons, k => ControllerState[(int)key.GamePadPlayerIndex][k]));
                }
                state = states.Max();
            }
            return state;
        }

        /// <summary>
        /// Simply checks if a rebindable key is down based on its function name.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static bool IsRebindableKeyPressed(KeyFunctions functionName)
        {
            return GetRebindableKeyState(functionName) != InputState.NONE;
        }

        /// <summary>
        /// Returns if a rebindable key was just pressed this frame.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static bool JustPressedRebindableKey(KeyFunctions functionName)
        {
            return GetRebindableKeyState(functionName) == InputState.PRESSED;
        }

        /// <summary>
        /// Simply checks if a key is down.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyPressed(Keys key)
        {
            return KeyState[key] != InputState.NONE;
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
            return KeyState[key] == InputState.PRESSED;
        }
    }
}
