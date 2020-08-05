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
                    ResetWriteArea();
                }
            }
        }

        public Vector2 WriteAreaSize
        {
            get
            {
                Rectangle writeArea = ActualWriteArea;
                return new Vector2(writeArea.Width, writeArea.Height);
            }
        }

        public bool CenteredText
        {
            get
            {
                return _centered;
            }
            set
            {
                _centered = value;
                ResetWriteArea();
            }
        }

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

        public char? PreviousCharacter
        {
            get
            {
                if (letterProgress > 0)
                {
                    return Text[letterProgress - 1];
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

        protected Rectangle ActualWriteArea { get; private set; }

        public int Speed { get; set; }

        public bool AtEndOfText { get; private set; }

        public bool AtEndOfBox { get; private set; }
        public bool WroteCharacter { get; private set; }

        public char LastWrittenCharacter { get; private set; }

        public bool DrawShadow { get; set; }

        public bool IgnoreSoftLineBreaks { get; set; }

        public DrawOrder drawLayer;

        public Point Position
        {
            get
            {
                return new Point(writeArea.X, WriteArea.Y);
            }
        }

        protected int letterProgress;

        protected List<List<TextCharacter>> characterLines;
        protected Vector2 cursorPos;
        protected Rectangle writeArea;
        protected SpriteFont spriteFont;
        protected Texture2D buttonSprite;

        public bool ForcedNewLine { get; private set; }

        private float _stepProgress;
        private bool _centered;
        private bool _newWord;
        private bool _newLine;

        private string _text;
        private float firstLineY;
        private int _line;

        public TextWriter()
        {
            Speed = DefaultTextSpeed;
            _text = "";

            letterProgress = 0;
            _stepProgress = 0;
            _newWord = false;
            _newLine = false;
            WroteCharacter = false;
            WriteArea = new Rectangle(0, 0, 200, 200);

            characterLines = new List<List<TextCharacter>>();

            drawLayer = DrawOrder.TEXT;
        }

        public TextWriter(int x, int y)
        {
            Speed = DefaultTextSpeed;
            _text = "";

            letterProgress = 0;
            _stepProgress = 0;
            _newWord = false;
            _newLine = false;
            WroteCharacter = false;
            WriteArea = new Rectangle(x, y, 1, 1);

            characterLines = new List<List<TextCharacter>>();

            drawLayer = DrawOrder.TEXT;
        }

        public TextWriter(int x, int y, int width, int height)
        {
            Speed = DefaultTextSpeed;
            _text = "";

            letterProgress = 0;
            _stepProgress = 0;
            _newWord = false;
            WroteCharacter = false;

            characterLines = new List<List<TextCharacter>>();

            WriteArea = new Rectangle(x, y, width, height);

            drawLayer = DrawOrder.TEXT;
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


        public void Initialize()
        {
            ResetWriteArea();
        }


        public void ResetWriteArea()
        {
            cursorPos = new Vector2();


            ActualWriteArea = WriteArea;
        }

        public void ResetTextProgress()
        {
            cursorPos = new Vector2();
            letterProgress = 0;
            _stepProgress = 0;
            _newWord = true;
            AtEndOfBox = false;
            AtEndOfText = false;
            characterLines.Clear();

            ResetWriteArea();
        }

        public void Move(int x, int y)
        {
            WriteArea = new Rectangle(x, y, WriteArea.Width, WriteArea.Height);
        }

        public void Draw()
        {
            float z = DrawingUtilities.GetDrawingZ(drawLayer);
            float shadowZ = z - 0.01f;
            foreach (List<TextCharacter> line in characterLines)
            {
                foreach (var c in line)
                {
                    if (c.Character == null)
                    {
                        SpriteDrawer.DrawGuiSprite(buttonSprite, c.Position, c.Crop, Z: z);
                    }
                    else
                    {
                        SpriteDrawer.DrawGuiSprite(spriteFont.texture, c.Position, c.Crop, spriteFont.color, Z: z);
                        if (DrawShadow)
                        {
                            SpriteDrawer.DrawGuiSprite(spriteFont.texture, c.Position + new Vector2(0, 1f), c.Crop, color: Color.Black, Z: shadowZ);
                        }
                    }
                }
            }
        }

        public void SkipCharacter()
        {
            letterProgress++;
        }

        public void Update()
        {
            WroteCharacter = false;
            ForcedNewLine = false;

            if (!AtEndOfBox && !AtEndOfText)
            {
                _stepProgress += GameTimes.DeltaTime * Speed;

                while (_stepProgress >= 1f)
                {
                    if (Text.Length != 0)
                    {
                        AtEndOfText = letterProgress == Text.Length;

                        if (!AtEndOfText)
                        {
                            AtEndOfBox = !Write(Text[letterProgress]);
                            WroteCharacter = true;
                        }
                    }
                    else
                    {
                        AtEndOfText = true;
                    }

                    _stepProgress--;
                }

            }
        }

        public float GetTextLenght()
        {
            float lenght = 0;
            for (int i = 0; i < Text.Length; i++)
            {
                int charWidth = spriteFont.spaceWidth;

                lenght += spriteFont.spaceWidth;
            }

            return lenght;
        }

        public float GetWordLenght()
        {
            float lenght = 0;

            int index = Text.IndexOfAny(WordBreaks, letterProgress);
            string word = Text.Substring(letterProgress, index != -1 ? index - letterProgress : Text.Length - letterProgress);
            for (int i = 0; i < word.Length; i++)
            {
                lenght += spriteFont.spaceWidth;
            }

            return lenght;
        }


        public int GetlineSeparation()
        {
            return spriteFont.lineSeparation;
        }

        public int GetEndYPos()
        {
            return (int)cursorPos.Y + spriteFont.lineSeparation;
        }

        public void SetWriteArea(int width, int height)
        {
            writeArea = new Rectangle(writeArea.X, writeArea.Y, width, height);
        }

        public void SetWriteArea(int x, int y, int width, int height)
        {
            writeArea = new Rectangle(x, y, width, height);
        }

        public void ProgressText()
        {
            if (!AtEndOfBox && !AtEndOfText)
            {
                AtEndOfText = letterProgress == Text.Length;

                if (!AtEndOfText)
                {
                    AtEndOfBox = !Write(Text[letterProgress]);
                }
            }
        }

        public void ProgressTextToEnd()
        {
            while (!AtEndOfText && !AtEndOfBox)
            {
                AtEndOfText = letterProgress == Text.Length;

                if (!AtEndOfText)
                {
                    AtEndOfBox = !Write(Text[letterProgress]);
                }
            }
        }

        public void RemoveFirstLine()
        {
            characterLines.RemoveAt(0);
            characterLines.Add(new List<TextCharacter>());
            _line--;
        }

        public void PushTextUp()
        {
            foreach (var line in characterLines)
            {
                foreach (var c in line)
                {
                    c.Position.Y -= spriteFont.lineSeparation / 2;
                }
            }

        }

        public void ResetCursor()
        {
            _newLine = true;
            cursorPos.X = 0;
            cursorPos.Y -= spriteFont.lineSeparation;
            AtEndOfBox = false;
            if (NextCharacter == ' ')
            {
                SkipCharacter();
            }
        }

        public void Rewite()
        {
            int oldprogress = letterProgress;

            ResetTextProgress();

            while (letterProgress != oldprogress)
            {
                AtEndOfText = letterProgress == Text.Length;

                if (!AtEndOfText)
                {
                    AtEndOfBox = !Write(Text[letterProgress]);
                }
            }
        }

        public void Stop()
        {
            WroteCharacter = false;
        }

        protected virtual bool Write(char character)
        {
            bool output = false;

            while (characterLines.Count-1 < _line)
            {
                characterLines.Add(new List<TextCharacter>());
            }

            if (character == ' ')
            {
                DoSpace();
                output = true;
            }
            else if ( LineBreaks.Any(c => c == character))
            {
                NewLine();
                ForcedNewLine = true;

                output = cursorPos.Y + spriteFont.lineSeparation < writeArea.Height;
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

                characterLines[_line].Add(new TextCharacter(null, new Vector2(cursorPos.X + writeArea.X, cursorPos.Y + writeArea.Y - 1), new Rectangle(pos % 13 * spaceWidth, pos / 13 * lineHeight, spaceWidth, lineHeight)));

                output = true;
            }
            else
            {
                Rectangle? rect = spriteFont.GetRectangle(character);
                Rectangle writeArea = ActualWriteArea;

                if (rect == null)
                {
                    DoSpace();
                    output = true;
                }
                else
                {
                    if (!KeepInBounds(rect.Value))
                    {
                        return false;
                    }
                    while (characterLines.Count - 1 < _line)
                    {
                        characterLines.Add(new List<TextCharacter>());
                    }


                    float y = cursorPos.Y + writeArea.Y + spriteFont.lineSeparation - rect.Value.Height;

                    characterLines[_line].Add(new TextCharacter(character, new Vector2(cursorPos.X + writeArea.X, y), rect));
                    LastWrittenCharacter = character;

                    letterProgress++;
                    output = true;

                    if (!IgnoreSoftLineBreaks && SoftLinebreak.Any(c => c == character) && letterProgress < Text.Length && Text[letterProgress] == ' ')
                    {
                        NewLine();
                        output = cursorPos.Y + spriteFont.lineSeparation < writeArea.Height;
                    }
                }
            }

            if (output)
            {
                LastWrittenCharacter = character;
            }

            return output;
        }

        protected void DoSpace()
        {
            _newWord = true;

            ProgressCursor();
            letterProgress++;
        }


        protected void NewLine()
        {
            cursorPos.Y += spriteFont.lineSeparation;
            cursorPos.X = 0;
            _newLine = true;
            _line++;


            letterProgress++;

            if (letterProgress < Text.Length && Text[letterProgress] == ' ')
            {
                letterProgress++;
            }
        }

        protected bool KeepInBounds(Rectangle characterRectangle)
        {
            bool nextLine = false;

            if (_newLine)
            {
                _newLine = false;
                return true;
            }
            else if (_newWord)
            {
                float wordLenght = GetWordLenght();

                if (wordLenght > writeArea.Width)
                {
                    nextLine = cursorPos.X + characterRectangle.Width > writeArea.Width;
                }
                else
                {
                    nextLine = cursorPos.X + wordLenght > writeArea.Width;
                }

                _newWord = false;
            }
            else
            {
                nextLine = cursorPos.X + characterRectangle.Width > writeArea.Width;
            }

            if (nextLine)
            {
                cursorPos.Y += spriteFont.lineSeparation;
                cursorPos.X = 0;
                _line++;

                return !(cursorPos.Y + spriteFont.lineSeparation > writeArea.Height);
            }
            else if(letterProgress > 0)
            {
                ProgressCursor();
            }

            return true;
        }

        protected void ProgressCursor()
        {
            if (characterLines.Count == 0)
            {
                return;
            }

            cursorPos.X += spriteFont.spaceWidth;
        }
    }
}
