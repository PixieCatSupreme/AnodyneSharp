using AnodyneSharp.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Resources.Loading
{
    public class InputConfigLoader : ContentLoader
    {
        public InputConfigLoader(string filePath)
            : base(filePath)
        { }

        public int RebindableKeys { get; private set; }

        public bool LoadInputConfig()
        {
            bool success = true;
            Dictionary<KeyFunctions, RebindableKey> rKeys = new Dictionary<KeyFunctions, RebindableKey>();

            while (!EndOfStream)
            {
                string[] lineParts = SplitNextLine();

                if (lineParts.Length == 1 &&
                    int.TryParse(lineParts[0], out int functionID))
                {
                    List<Keys> keys = new List<Keys>();
                    List<Buttons> buttons = new List<Buttons>();

                    PlayerIndex playerIndex = (PlayerIndex)(-1);

                    lineParts = SplitNextLine();
                    if (lineParts[0] != "}")
                    {
                        lineParts = SplitNextLine();

                        keys = ReadKeys(lineParts);

                        if (keys.Count > 0)
                        {
                            lineParts = SplitNextLine();
                        }

                        buttons = ReadButtons(lineParts, out playerIndex);
                    }

                    rKeys.Add((KeyFunctions)functionID, new RebindableKey(keys, buttons, playerIndex));

                    SplitNextLine();
                }
                else
                {
                    ThrowFileError("Unable to parse input data line!");
                    success = false;
                    break;
                }
            }

            if (success)
            {
                KeyInput.RebindableKeys = rKeys;
            }

            return success;
        }

        protected List<Keys> ReadKeys(string[] keyLine)
        {
            List<Keys> keys = new List<Keys>();

            if (keyLine[0] == "k")
            {
                keyLine = SplitNextLine();
                if (keyLine[0] == "{")
                {
                    while (SplitNextLine()[0] != "}")
                    {
                        keyLine = CurrentLine;

                        if (int.TryParse(keyLine[0], out int keyID))
                        {
                            keys.Add((Keys)keyID);
                        }
                        else
                        {
                            ThrowFileError($"Unable to read key!");
                            return new List<Keys>(); ;
                        }
                    }
                }
            }
            return keys;
        }

        protected List<Buttons> ReadButtons(string[] buttonLine, out PlayerIndex playerIndex)
        {
            List<Buttons> buttons = new List<Buttons>();
            playerIndex = (PlayerIndex)(-1);

            if (buttonLine[0] == "b" && int.TryParse(buttonLine[1], out int playerIndexID))
            {
                buttonLine = SplitNextLine();
                if (buttonLine[0] == "{")
                {
                    while (SplitNextLine()[0] != "}")
                    {
                        buttonLine = CurrentLine;

                        if (int.TryParse(buttonLine[0], out int buttonID))
                        {
                            buttons.Add((Buttons)buttonID);
                        }
                        else
                        {
                            ThrowFileError($"Unable to read button!");
                            return new List<Buttons>(); ;
                        }
                    }

                    playerIndex = (PlayerIndex)playerIndexID;
                }
            }
            return buttons;
        }
    }
}
