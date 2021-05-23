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
            return Enum.GetName(typeof(Keys), key)
                .Replace("PageUp", "PgUp")
                .Replace("PageDown", "PgDwn")
                .Replace("LeftS", "LShft")
                .Replace("RightShift", "RShft")
                .Replace("LeftA", "LA")
                .Replace("RightA", "RA")
                .Replace("LeftC", "LC")
                .Replace("RightC", "RC")
                .Replace("Escape", "Esc");
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
                Buttons.LeftTrigger => 8,
                Buttons.RightTrigger => 9,

                Buttons.LeftStick => 10,
                Buttons.RightStick => 11,

                Buttons.LeftThumbstickUp => 13,
                Buttons.LeftThumbstickDown => 14,
                Buttons.LeftThumbstickLeft => 15,
                Buttons.LeftThumbstickRight => 16,
                Buttons.RightThumbstickUp => 17,
                Buttons.RightThumbstickDown => 18,
                Buttons.RightThumbstickLeft => 19,
                Buttons.RightThumbstickRight => 20,
                Buttons.DPadUp => 21,
                Buttons.DPadDown => 22,
                Buttons.DPadLeft => 23,
                Buttons.DPadRight => 24,
                _ => -1
            };

            return "♦" + output + "♦";
        }
    }
}
