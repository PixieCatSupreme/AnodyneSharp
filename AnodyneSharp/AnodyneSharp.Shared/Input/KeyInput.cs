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
        NextPage,
        Broom1,
        Broom2,
        Broom3,
        Broom4
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

        private enum ControllerKind
        {
            XBOX,
            NINTENDO,
            SONY
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

        public static bool ControllerMode { get; private set; }
        public static bool ControllerModeChanged { get; private set; }

        public static int ControllerButtonOffset { get; private set; }

        private static bool _faceButtonsSwitched;

        private static ControllerKind CurrentControlerKind;

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
            ControllerModeChanged = false;

            KeyboardState s = Keyboard.GetState();

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                bool isDown = s.IsKeyDown(key);
                KeyState[key] = UpdateInput(KeyState[key], s.IsKeyDown(key));

                if (ControllerMode)
                {
                    ControllerMode = !isDown;
                    ControllerModeChanged = true;
                }
            }

            for (int i = 0; i < ControllerState.Length; i++)
            {
                GamePadState g = GamePad.GetState(i);

                var dic = ControllerState[i];

                foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
                {
                    bool isDown = g.IsButtonDown(button);
                    dic[button] = UpdateInput(dic[button], isDown);

                    if (!ControllerMode)
                    {
                        ControllerMode = isDown;
                        ControllerModeChanged = true;
                    }
                }
            }

            if (ControllerModeChanged)
            {
                string displayName = (GamePad.GetCapabilities(PlayerIndex.One).DisplayName ?? "").ToLower();

                if (displayName.Contains("nintendo"))
                {
                    ControllerButtonOffset = 26;
                    CurrentControlerKind = ControllerKind.NINTENDO;
                    if (!_faceButtonsSwitched)
                    {
                        SwapFaceButtons();
                    }
                }
                else if (displayName.Contains("sony") || displayName.Contains("playstation") || 
                    displayName.Contains("ps3") || displayName.Contains("ps4") || displayName.Contains("ps5"))
                {
                    ControllerButtonOffset = 52;
                    CurrentControlerKind = ControllerKind.SONY;
                    if (_faceButtonsSwitched)
                    {
                        SwapFaceButtons();
                    }
                }
                else
                {
                    ControllerButtonOffset = 0;
                    CurrentControlerKind = ControllerKind.XBOX;
                    if (_faceButtonsSwitched)
                    {
                        SwapFaceButtons();
                    }
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
        /// Checks if any key is down, returns true and outputs the pressed key.
        /// </summary>
        /// <param name="pressed"></param>
        /// <returns></returns>
        public static bool IsAnyKeyPressed(out Keys? pressed)
        {
            pressed = null;

            if (!KeyState.Any(k => k.Value == InputState.PRESSED))
            {
                return false;
            }

            pressed = KeyState.FirstOrDefault(k => k.Value == InputState.PRESSED).Key;

            return true;
        }

        public static bool IsAnyButtonPressed(out Buttons? pressed)
        {
            pressed = null;

            var t = ControllerState.Where(c => c.Any(b => b.Value == InputState.PRESSED));

            if (!t.Any())
            {
                return false;
            }

            pressed = t.First().First(b => b.Value == InputState.PRESSED).Key;

            return true;
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

        public static void SwapFaceButtons()
        {
            _faceButtonsSwitched = !_faceButtonsSwitched;

            var aList = RebindableKeys.Where(k => k.Value.Buttons.Contains(Buttons.A)).ToList();
            var bList = RebindableKeys.Where(k => k.Value.Buttons.Contains(Buttons.B)).ToList();
            var xList = RebindableKeys.Where(k => k.Value.Buttons.Contains(Buttons.X)).ToList();
            var yList = RebindableKeys.Where(k => k.Value.Buttons.Contains(Buttons.Y)).ToList();

            foreach (var item in aList)
            {
                item.Value.Buttons.Remove(Buttons.A);
                item.Value.Buttons.Add(Buttons.B);
            }

            foreach (var item in bList)
            {
                item.Value.Buttons.Remove(Buttons.B);
                item.Value.Buttons.Add(Buttons.A);
            }

            foreach (var item in xList)
            {
                item.Value.Buttons.Remove(Buttons.X);
                item.Value.Buttons.Add(Buttons.Y);
            }

            foreach (var item in yList)
            {
                item.Value.Buttons.Remove(Buttons.Y);
                item.Value.Buttons.Add(Buttons.X);
            }
        }
    }
}
