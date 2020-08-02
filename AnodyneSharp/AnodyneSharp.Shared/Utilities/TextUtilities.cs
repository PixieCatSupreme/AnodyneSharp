using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Utilities
{
    public class TextUtilities
    {
        public static string GetKeyBoardString(Keys key)
        {
            return Enum.GetName(typeof(Keys), key);
        }

        public  static string GetButtonString(Buttons button)
        {
            int output = button switch
            {
                Buttons.A => 0,
                Buttons.B => 1,
                Buttons.X => 2,
                Buttons.Y => 3,
                Buttons.Start => 4,
                Buttons.Back => 5,
                Buttons.LeftShoulder => 6,
                Buttons.RightShoulder => 7,
                Buttons.RightTrigger => 8,
                Buttons.LeftTrigger => 9,
                Buttons.LeftStick => 10,
                Buttons.RightStick => 11,
                Buttons.LeftThumbstickLeft => 13,
                Buttons.RightThumbstickUp => 14,
                Buttons.RightThumbstickDown => 15,
                Buttons.RightThumbstickRight => 16,
                Buttons.RightThumbstickLeft => 17,
                Buttons.LeftThumbstickUp => 18,
                Buttons.LeftThumbstickDown => 20,
                Buttons.LeftThumbstickRight => 21,
                Buttons.DPadUp => 22,
                Buttons.DPadDown => 23,
                Buttons.DPadLeft => 24,
                Buttons.DPadRight => 25,
                _ => -1
            };

            return "♦" + output + "♦";
        }
    }
}
