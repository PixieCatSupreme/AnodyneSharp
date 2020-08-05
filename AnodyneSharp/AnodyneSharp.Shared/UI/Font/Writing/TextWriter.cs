using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpriteFont = AnodyneSharp.UI.Font.SpriteFont;

namespace AnodyneSharp.UI.Text
{
    public class TextWriter
    {
        private const int DefaultTextSpeed = 30;
        public static char[] LineBreaks = new char[] { '\n', '\r' };
        public static char[] SoftLinebreak = new char[] { '.', '!', '。', '…', '？', '！', '?' };
        public static char[] WordBreaks = new char[] { ' ', '¶', '\n' };

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = Regex.Unescape(value).Trim();
                ResetTextProgress();
            }
        }

        public Rectangle WriteArea
        {
            get
            {
                return writeArea;
            }
            set
            {
                if (writeArea != value)
                {
                    SetWriteArea(value.X, value.Y, value.Width, value.Height);
                }
            }
        }

        public Vector2 WriteAreaTopLeft => new Vector2(writeArea.X, writeArea.Y);

        public Vector2 WriteAreaSize => new Vector2(writeArea.Width, writeArea.Height);

        public char? NextCharacter
        {
            get
            {
                if (letterProgress < Text.Length)
                {
                    return Text[letterProgress];
                }
                else
                {
                    return null;
                }
            }
        }

        public bool FirstLineEmpty
        {
            get
            {
                return !characterLines.FirstOrDefault()?.Any() ?? true;
            }
        }

        public int Speed;

        public bool AtEndOfText => letterProgress == Text.Length;
        public bool AtEndOfBox => _line >= LinesPerBox;

        public bool DrawShadow { get; set; }

        public bool IgnoreSoftLineBreaks { get; set; }

        public int LinesPerBox => writeArea.Height / spriteFont.lineSeparation;

        public DrawOrder drawLayer;

        public Point Position
        {
            get
            {
                return new Point(writeArea.X, WriteArea.Y);
            }
        }

        private int letterProgress;

        private List<List<TextCharacter>> characterLines;
        private int _line;
        private float currentLineWidth;
        private Rectangle writeArea;
        private SpriteFont spriteFont;
        private Texture2D buttonSprite;

        private float _stepProgress;
        private bool _newWord;

        private string _text;
        private float firstLineY;

        public TextWriter() : this(0, 0, 200, 200)
        { }

        public TextWriter(int x, int y) : this(x, y, 1, 1)
        { }

        public TextWriter(int x, int y, int width, int height)
        {
            Speed = DefaultTextSpeed;
            _text = "";

            characterLines = new List<List<TextCharacter>>();

            WriteArea = new Rectangle(x, y, width, height);

            drawLayer = DrawOrder.TEXT;

            ResetTextProgress();
        }

        public void SetSpriteFont(SpriteFont font, Texture2D buttonSprite)
        {
            spriteFont = font;
            this.buttonSprite = buttonSprite;
        }


        public int GetLineHeight()
        {
            return spriteFont.lineHeight;
        }

        public void ResetTextProgress()
        {
            currentLineWidth = 0;
            letterProgress = 0;
            _stepProgress = 0;
            _newWord = true;
            characterLines.Clear();
            _line = 0;
            firstLineY = 0;
        }

        public void Draw()
        {
            float z = DrawingUtilities.GetDrawingZ(drawLayer);
            float shadowZ = z - 0.01f;
            float currentY = firstLineY;
            foreach (List<TextCharacter> line in characterLines)
            {
                foreach (var c in line)
                {
                    if (c.Character == null)
                    {
                        SpriteDrawer.DrawGuiSprite(buttonSprite, WriteAreaTopLeft + new Vector2(c.X, currentY), c.Crop, Z: z);
                    }
                    else
                    {
                        SpriteDrawer.DrawGuiSprite(spriteFont.texture, WriteAreaTopLeft + new Vector2(c.X, currentY), c.Crop, spriteFont.color, Z: z);
                        if (DrawShadow)
                        {
                            SpriteDrawer.DrawGuiSprite(spriteFont.texture, WriteAreaTopLeft + new Vector2(c.X, currentY + 1), c.Crop, color: Color.Black, Z: shadowZ);
                        }
                    }
                }
                currentY += spriteFont.lineSeparation;
            }
        }

        public void SkipCharacter()
        {
            letterProgress++;
        }

        public void Update()
        {
            if (!AtEndOfBox && !AtEndOfText)
            {
                _stepProgress += GameTimes.DeltaTime * Speed;

                while (_stepProgress >= 1f)
                {
                    _stepProgress--;

                    ProgressText();

                }

            }
        }

        public void ProgressText()
        {
            if (!AtEndOfBox && !AtEndOfText)
            {
                Write(Text[letterProgress]);
            }
        }

        public void ProgressTextToEnd()
        {
            while (!AtEndOfText && !AtEndOfBox)
            {
                ProgressText();
            }
        }

        public float GetTextLength()
        {
            return Text.Length * spriteFont.spaceWidth;
        }

        public float GetWordLength()
        {
            int index = Text.IndexOfAny(WordBreaks, letterProgress);
            string word = Text.Substring(letterProgress, index != -1 ? index - letterProgress : Text.Length - letterProgress);

            return word.Length * spriteFont.spaceWidth;
        }

        public void SetWriteArea(int width, int height)
        {
            writeArea = new Rectangle(writeArea.X, writeArea.Y, width, height);
        }

        public void SetWriteArea(int x, int y, int width, int height)
        {
            writeArea = new Rectangle(x, y, width, height);
        }

        public void RemoveFirstLine()
        {
            characterLines.RemoveAt(0);
            _line--;
            firstLineY += spriteFont.lineSeparation;
        }

        public void PushTextUp()
        {
            firstLineY -= spriteFont.lineSeparation / 2;
        }

        private bool Write(char character)
        {
            bool output = false;

            while (characterLines.Count - 1 < _line)
            {
                characterLines.Add(new List<TextCharacter>());
            }

            if (character == ' ')
            {
                DoSpace();
                output = true;
            }
            else if (LineBreaks.Any(c => c == character))
            {
                NewLine();

                output = _line < LinesPerBox;
            }
            //♦ is used to indicate that the next 'character' is a button
            else if (character == '♦')
            {
                letterProgress++;

                string s = Text.Substring(letterProgress, Text.IndexOf('♦', letterProgress + 1) - letterProgress);

                letterProgress += s.Length + 1;

                int pos = int.Parse(s) + KeyInput.ControllerButtonOffset;
                int spaceWidth = 13;
                int lineHeight = 14;

                characterLines[_line].Add(new TextCharacter(null, currentLineWidth, new Rectangle(pos % 13 * spaceWidth, pos / 13 * lineHeight, spaceWidth, lineHeight)));

                output = true;
            }
            else
            {
                Rectangle? rect = spriteFont.GetRectangle(character);

                if (!KeepInBounds(rect.Value))
                {
                    return false;
                }
                while (characterLines.Count - 1 < _line)
                {
                    characterLines.Add(new List<TextCharacter>());
                }

                characterLines[_line].Add(new TextCharacter(character, currentLineWidth, rect));

                letterProgress++;
                output = true;

                if (!IgnoreSoftLineBreaks && SoftLinebreak.Any(c => c == character) && letterProgress < Text.Length && Text[letterProgress] == ' ')
                {
                    NewLine();
                    output = _line < LinesPerBox;
                }
            }

            return output;
        }

        private void DoSpace()
        {
            _newWord = true;

            ProgressCursor();
            letterProgress++;
        }


        private void NewLine()
        {
            currentLineWidth = 0;
            _line++;


            letterProgress++;

            if (letterProgress < Text.Length && Text[letterProgress] == ' ')
            {
                letterProgress++;
            }
        }

        private bool KeepInBounds(Rectangle characterRectangle)
        {
            if (characterLines[_line].Count == 0)
            {
                return true;
            }

            bool nextLine;
            if (_newWord)
            {
                float wordLength = GetWordLength();

                if (wordLength > writeArea.Width)
                {
                    nextLine = currentLineWidth + characterRectangle.Width > writeArea.Width;
                }
                else
                {
                    nextLine = currentLineWidth + wordLength > writeArea.Width;
                }

                _newWord = false;
            }
            else
            {
                nextLine = currentLineWidth + characterRectangle.Width > writeArea.Width;
            }

            if (nextLine)
            {
                currentLineWidth = 0;
                _line++;

                return !AtEndOfBox;
            }
            else if (letterProgress > 0)
            {
                ProgressCursor();
            }

            return true;
        }

        private void ProgressCursor()
        {
            currentLineWidth += spriteFont.spaceWidth;
        }
    }
}
