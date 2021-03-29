using AnodyneSharp.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Resources.Writing
{
    public class InputConfigWriter : ContentWriter
    {
        public InputConfigWriter(string filePath)
            : base(filePath)
        { }

        public bool WriteInputConfig()
        {
            bool result = true;

            foreach (var rKey in KeyInput.RebindableKeys)
            {
                WriteLine(((int)rKey.Key).ToString());

                WriteLine('{');

                WriteKeys(rKey.Value);

                WriteButtons(rKey.Value);

                WriteLine('}');
            }

            return result;
        }


        private void WriteKeys(RebindableKey rKey)
        {
            WriteLine("\tk");

            WriteLine("\t{");

            foreach (var key in rKey.Keys)
            {
                WriteLine($"\t\t{((int)key).ToString()}");
            }

            WriteLine("\t}");
        }

        private void WriteButtons(RebindableKey rKey)
        {
            WriteLine($"\tb\t{(int)rKey.GamePadPlayerIndex}");

            WriteLine("\t{");

            foreach (var button in rKey.Buttons)
            {
                WriteLine($"\t\t{((int)button).ToString()}");
            }

            WriteLine("\t}");
        }
    }
}