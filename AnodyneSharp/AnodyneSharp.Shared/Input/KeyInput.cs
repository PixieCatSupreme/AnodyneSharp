using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
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
        public static List<RebindableKey> RebindableKeys;

        private static TouchCollection currentTouchState;
        private static GestureSample? currentGesture;
        private static bool AnyUIButtonDown;

        private static MouseState oldMouseState;
        private static MouseState mouseState;
        private static List<RebindableKey> pressedRebindableKeys;
        private static List<Keys> pressedKeys;
        private static List<PlayerButton> pressedButtons;
        private static List<MouseButton> pressedMouseButton;
        private static ButtonState[] mouseButtonStates;
        private static ButtonState[] oldMouseButtonStates;

        public static Vector2 LastCursorPos;

        static KeyInput()
        {
            RebindableKeys = new List<RebindableKey>();
            pressedRebindableKeys = new List<RebindableKey>();

            currentTouchState = new TouchCollection();
            currentGesture = null;
            AnyUIButtonDown = false;

            pressedKeys = new List<Keys>();
            pressedButtons = new List<PlayerButton>();
            pressedMouseButton = new List<MouseButton>();
            mouseButtonStates = new ButtonState[(int)MouseButton.MiddleButton + 1];
            oldMouseButtonStates = new ButtonState[(int)MouseButton.MiddleButton + 1];
        }

        /// <summary>
        /// Updates the mouse state and the pressed mouse buttons. Also saves the previous state to check if buttons are pressed or released.
        /// </summary>
        /// <param name="state"></param>
        public static void UpdateMouseState(MouseState state)
        {
            oldMouseState = mouseState;
            mouseState = state;
            mouseButtonStates[(int)MouseButton.LeftButton] = state.LeftButton;
            mouseButtonStates[(int)MouseButton.RightButton] = state.RightButton;
            mouseButtonStates[(int)MouseButton.MiddleButton] = state.MiddleButton;
            oldMouseButtonStates[(int)MouseButton.LeftButton] = oldMouseState.LeftButton;
            oldMouseButtonStates[(int)MouseButton.RightButton] = oldMouseState.RightButton;
            oldMouseButtonStates[(int)MouseButton.MiddleButton] = oldMouseState.MiddleButton;

            LastCursorPos = new Vector2(state.Position.X, state.Position.Y) /*/ DrawingValues.Scale*/;
        }

        /// <summary>
        /// Updates the mouse state and the pressed mouse buttons. Also saves the previous state to check if buttons are pressed or released.
        /// </summary>
        /// <param name="state"></param>
        public static void UpdateTouchState()
        {
            AnyUIButtonDown = false;

            currentTouchState = TouchPanel.GetState();
            TouchPanel.EnabledGestures = GestureType.Pinch | GestureType.Hold | GestureType.Tap;

            if (TouchPanel.EnabledGestures != GestureType.None)
            {
                currentGesture = TouchPanel.IsGestureAvailable ? TouchPanel.ReadGesture() : new GestureSample?();
            }
            else if (currentGesture != null)
            {
                currentGesture = new GestureSample?();
            }
        }

        /// <summary>
        /// Simply checks if a rebindable key is down based on its function name.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static bool IsRebindableKeyPressed(Enum functionName)
        {
            RebindableKey key = RebindableKeys.FirstOrDefault(rk => CompareFunctionNames(rk.FunctionName, functionName));

            if (key.FunctionName == null)
            {
                return false;
            }
            else if (IsAnyKeyPressed(key.Keys))
            {
                return true;
            }
            else if (IsAnyButtonPressed(key.Buttons))
            {
                return true;
            }
            else if (IsAnyMouseButtonPressed(key.MouseButtons))
            {
                return true;
            }
            else if (IsAnyGestureHappening(key.TouchGestures))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns if a rebindable key is not pressed yet and if so, adds the key to the list of pressed rebindable keys.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static bool CanPressRebindableKey(Enum functionName)
        {
            if (!RebindableKeys.Any(rk => CompareFunctionNames(rk.FunctionName, functionName)))
            {
                return false;
            }
            else if (IsRebindableKeyPressed(functionName))
            {
                if (!IsRebindableKeyAlreadyDown(functionName))
                {
                    RebindableKey key = RebindableKeys.FirstOrDefault(rk => CompareFunctionNames(rk.FunctionName, functionName));

                    pressedRebindableKeys.Add(key);
                    return true;
                }
            }
            else if (IsRebindableKeyAlreadyDown(functionName))
            {
                ReleaseRebindableKey(functionName);
            }
            return false;
        }

        /// <summary>
        /// Checks if a key is already in the list of pressed keys.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private static bool IsRebindableKeyAlreadyDown(Enum functionName)
        {
            return pressedRebindableKeys.Any(rk => CompareFunctionNames(rk.FunctionName, functionName));
        }

        /// <summary>
        /// Release the rebindable key by removing it from the list of pressed rebindable keys.
        /// </summary>
        /// <param name="functionName"></param>
        private static void ReleaseRebindableKey(Enum functionName)
        {
            RebindableKey key = RebindableKeys.FirstOrDefault(rk => CompareFunctionNames(rk.FunctionName, functionName));

            if (key.FunctionName == null)
            {
                return;
            }
            else if (pressedRebindableKeys.Contains(key))
            {
                pressedRebindableKeys.Remove(key);
            }
        }

        /// <summary>
        /// Simply checks if a key is down.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsKeyPressed(Keys key)
        {
            return Keyboard.GetState().IsKeyDown(key);
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
                if (Keyboard.GetState().IsKeyDown(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns if a key is not pressed yet and if so, adds the key to the list of pressed keys.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CanPressKey(Keys key)
        {
            if (IsKeyPressed(key))
            {
                if (!IsKeyAlreadyDown(key))
                {
                    pressedKeys.Add(key);
                    return true;
                }
            }
            else if (IsKeyAlreadyDown(key))
            {
                ReleaseKey(key);
            }
            return false;
        }

        /// <summary>
        /// Checks if a key is already in the list of pressed keys.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool IsKeyAlreadyDown(Keys key)
        {
            return pressedKeys.Count != 0 && pressedKeys.Contains(key) && pressedKeys.First(k => k == key) != Keys.None;
        }

        /// <summary>
        /// Release the key by removing it from the list of pressed keys.
        /// </summary>
        /// <param name="key"></param>
        private static void ReleaseKey(Keys key)
        {
            pressedKeys.Remove(key);
        }

        /// <summary>
        /// Simply checks if a button is down.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsButtonPressed(Buttons button, PlayerIndex playerIndex = PlayerIndex.One)
        {
            return GamePad.GetState(playerIndex).IsButtonDown(button);
        }

        /// <summary>
        /// Simply checks if any of the buttons in the list is down.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsAnyButtonPressed(List<Buttons> buttons, PlayerIndex playerIndex = PlayerIndex.One)
        {
            foreach (var button in buttons)
            {
                var state = GamePad.GetState(playerIndex);
                if (GamePad.GetState(playerIndex).IsButtonDown(button))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns if a button is not pressed yet and if so, adds the button to the list of pressed buttons.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool CanPressButton(Buttons button, PlayerIndex playerIndex = PlayerIndex.One)
        {
            if (IsButtonPressed(button, playerIndex))
            {
                if (!IsButtonAlreadyDown(button, playerIndex))
                {
                    pressedButtons.Add(new PlayerButton(button, playerIndex));
                    return true;
                }
            }
            else if (IsButtonAlreadyDown(button, playerIndex))
            {
                ReleaseButton(button);
            }
            return false;
        }

        /// <summary>
        /// Checks if a button is already in the list of pressed buttons.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private static bool IsButtonAlreadyDown(Buttons button, PlayerIndex playerIndex = PlayerIndex.One)
        {
            return pressedButtons.Count != 0 && pressedButtons.Any(pb => pb.Button == button);
        }

        /// <summary>
        /// Release the button by removing it from the list of pressed buttons.
        /// </summary>
        /// <param name="button"></param>
        private static void ReleaseButton(Buttons button, PlayerIndex playerIndex = PlayerIndex.One)
        {
            PlayerButton pb = pressedButtons.FirstOrDefault(b => b.Button == button && b.CurrentPlayerIndex == playerIndex);
            if (pb.Button != 0)
            {
                pressedButtons.Remove(pb);
            }

        }

        /// <summary>
        /// Checks if the mouse button is currently being pressed. Not the same as a button being down.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsMouseButtonPressed(MouseButton button)
        {
            return IsMouseButtonDown(button) && oldMouseButtonStates[(int)button] == ButtonState.Released;
        }

        /// <summary>
        /// Checks if any of the mouse buttons is currently being pressed. Not the same as a button being down.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsAnyMouseButtonPressed(List<MouseButton> buttons)
        {
            foreach (var button in buttons)
            {
                if (IsMouseButtonDown(button) && oldMouseButtonStates[(int)button] == ButtonState.Released)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a pressed button is being released.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsMouseButtonReleased(MouseButton button)
        {
            return !IsMouseButtonDown(button) && oldMouseButtonStates[(int)button] == ButtonState.Pressed;
        }

        /// <summary>
        /// Returns if a button is not pressed yet.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool CanPressMouseButton(MouseButton button)
        {
            if (IsMouseButtonDown(button))
            {
                return !IsMouseButtonAlreadyDown(button);
            }
            return false;
        }

        /// <summary>
        /// Simply checks if the mouse button is down.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool IsMouseButtonDown(MouseButton button)
        {
            return mouseButtonStates[(int)button] == ButtonState.Pressed;
        }

        /// <summary>
        /// Checks if a mouse button is already since last state update.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private static bool IsMouseButtonAlreadyDown(MouseButton button)
        {
            return mouseButtonStates[(int)button] == oldMouseButtonStates[(int)button];
        }

        /// <summary>
        /// Gets all current screen touches.
        /// </summary>
        /// <returns></returns>
        public static List<TouchLocation> GetCurrentTouchLocations()
        {
            return currentTouchState.ToList();
        }

        /// <summary>
        /// Gets the touch with the id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TouchLocation? GetTouchOfId(int id)
        {
            return currentTouchState.FirstOrDefault(t => t.Id == id);
        }


        /// <summary>
        /// Checks if the touch with the id is released or not.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsTouchReleased(int id)
        {
            TouchLocation? touch = GetTouchOfId(id);
            return touch == null || touch.Value.State == TouchLocationState.Released;
        }

        /// <summary>
        /// Checks if a certian gesture is happening and no UI button is being pressed.
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        public static bool IsGestureHappening(GestureType gesture)
        {
            return !AnyUIButtonDown && currentGesture != null && currentGesture.Value.GestureType == gesture;
        }

        /// <summary>
        /// Checks if any of the gestures are happening and no UI button is being pressed.
        /// </summary>
        /// <param name="gestures"></param>
        /// <returns></returns>
        public static bool IsAnyGestureHappening(List<GestureType> gestures)
        {
            if (AnyUIButtonDown || currentGesture == null)
            {
                return false;
            }

            foreach (var gesture in gestures)
            {
                if (currentGesture.Value.GestureType == gesture)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This method gets called when any UI button is being pressed to prevent gestures from being listened to.
        /// </summary>
        /// <returns></returns>
        public static void SetUIButtonDown()
        {
            AnyUIButtonDown = true;
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
