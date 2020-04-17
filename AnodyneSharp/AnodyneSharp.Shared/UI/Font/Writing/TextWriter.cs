using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using SpriteFont = AnodyneSharp.UI.Font.SpriteFont;

namespace AnodyneSharp.UI.Text
{
    public class TextWriter
    {
        private const int DefaultTextSpeed = 30;
        private static char[] LineBreaks = new char[] { '\n', '\r'};
        private static char[] WordBreaks = new char[] { ' ', '¶', '\n' };

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
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

        protected Rectangle ActualWriteArea { get; private set; }

        public int Speed { get; set; }

        public bool AtEndOfText { get; private set; }
        public bool AtEndOfBox { get; private set; }
        public bool WroteCharacter { get; private set; }

        public char LastWrittenCharacter { get; private set; }

        public Point Position
        {
            get
            {
                return new Point(writeArea.X, WriteArea.Y);
            }
        }

        protected int letterProgress;

        protected List<TextCharacter> characters;
        protected Vector2 cursorPos;
        protected Rectangle writeArea;
        protected SpriteFont spriteFont;

        private float _stepProgress;
        private float _scale;
        private bool _centered;
        private bool _newWord;
        private bool _newLine;

        private string _text;
        private float firstLineY;

        public TextWriter()
        {
            Speed = DefaultTextSpeed;
            _text = "";

            letterProgress = 0;
            _stepProgress = 0;
            _scale = 1;
            _newWord = false;
            _newLine = false;
            WroteCharacter = false;
            WriteArea = new Rectangle(0, 0, 200, 200);

            characters = new List<TextCharacter>();
        }

        public TextWriter(int x, int y)
        {
            Speed = DefaultTextSpeed;
            _text = "";

            letterProgress = 0;
            _stepProgress = 0;
            _scale = 1;
            _newWord = false;
            _newLine = false;
            WroteCharacter = false;
            WriteArea = new Rectangle(x, y, 1, 1);

            characters = new List<TextCharacter>();
        }

        public TextWriter(int x, int y, int width, int height)
        {
            Speed = DefaultTextSpeed;
            _text = "";

            letterProgress = 0;
            _stepProgress = 0;
            _scale = 1;
            _newWord = false;
            WroteCharacter = false;

            characters = new List<TextCharacter>();

            WriteArea = new Rectangle(x, y, width, height);
        }

        public void SetSpriteFont(SpriteFont font)
        {
            spriteFont = font;
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
            characters.Clear();

            ResetWriteArea();
        }

        public void Move(int x, int y)
        {
            WriteArea = new Rectangle(x, y, WriteArea.Width, WriteArea.Height);
        }

        public void Draw()
        {
            float z = DrawingUtilities.GetDrawingZ(DrawOrder.TEXT);
            float shadowZ = z - 0.01f;
            foreach (var c in characters)
            {
                SpriteDrawer.DrawGuiSprite(spriteFont.texture, c.Position, c.Crop, Z: z);
                SpriteDrawer.DrawGuiSprite(spriteFont.texture, c.Position + new Vector2(0, 1f), c.Crop, color: Color.Black, Z: shadowZ);

            }
        }

        public void SkipCharacter()
        {
            letterProgress++;
        }

        public void Update()
        {
            WroteCharacter = false;

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

        public int GetLineHeight()
        {
            return spriteFont.lineHeight;
        }

        public int GetEndYPos()
        {
            return (int)cursorPos.Y + spriteFont.lineHeight;
        }

        public void SetWriteArea(int width, int height)
        {
            writeArea = new Rectangle(writeArea.X , writeArea.Y , width, height);
        }

        public void SetWriteArea(int x, int y, int width, int height)
        {
            writeArea = new Rectangle(x , y , width, height);
        }

        public void ProgressText()
        {
            if (!AtEndOfBox && !AtEndOfText)
            {
                AtEndOfText = letterProgress != Text.Length;

                if (!AtEndOfText)
                {
                    AtEndOfBox = !Write(Text[letterProgress]);
                }
            }
        }

        public void RemoveFirstLine()
        {
            firstLineY = characters.First().Position.Y;

            for (int i = characters.Count-1; i <= 0; i++)
            {
                var c = characters[i];
                if (c.Position.Y == firstLineY)
                {
                    characters.Remove(c);
                }
            }
        }

        public bool PushTextUp()
        {
            bool finished = false;
            foreach (var c in characters)
            {
                if (finished)
                {
                    c.Position.Y = firstLineY;
                }
                else
                {
                    c.Position.Y -= GameTimes.DeltaTime;

                    if (c.Position.Y <= firstLineY)
                    {
                        c.Position.Y = firstLineY;
                        finished = true;
                    }
                }
            }

            return finished;
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

            if (character == ' ')
            {
                _newWord = true;

                ProgressCursor();
                letterProgress++;
                output =  true;
            }
            else if (LineBreaks.Any(c => c == character))
            {
                letterProgress++;

                cursorPos.Y += spriteFont.lineHeight;
                cursorPos.X = 0;
                _newLine = true;

                output =  cursorPos.Y + spriteFont.lineHeight < writeArea.Height;
            }
            else
            {
                Rectangle? rect = spriteFont.GetRectangle(character);
                Rectangle writeArea = ActualWriteArea;

                if (!KeepInBounds(rect.Value))
                {
                    return false;
                }

                float y = cursorPos.Y + writeArea.Y + spriteFont.lineHeight - rect.Value.Height;

                characters.Add(new TextCharacter(character, new Vector2(cursorPos.X + writeArea.X, y), rect));
                LastWrittenCharacter = character;

                letterProgress++;
                output =  true;
            }

            if (output)
            {
                LastWrittenCharacter = character;
            }

            return output;
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
                cursorPos.Y += spriteFont.lineHeight;
                cursorPos.X = 0;

                return !(cursorPos.Y + spriteFont.lineHeight > writeArea.Height);
            }
            else
            {
                ProgressCursor();
            }

            return true;
        }

        protected void ProgressCursor()
        {
            if (characters.Count == 0)
            {
                return;
            }

            cursorPos.X += spriteFont.spaceWidth;
        }

        private Rectangle GetCenteredDrawingArea()
        {
            float textWidth = GetTextLenght();
            int width = WriteArea.Width;
            int height = WriteArea.Height;
            return new Rectangle((int)(WriteArea.X + width / 2 - textWidth / 2), WriteArea.Y + height / 2 - (spriteFont != null ? spriteFont.lineHeight / 2 : 0), writeArea.Width, writeArea.Height);
        }
    }
}
