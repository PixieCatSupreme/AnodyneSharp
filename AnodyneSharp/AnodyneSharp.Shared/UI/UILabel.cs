using AnodyneSharp.UI.Text;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.UI
{
    public class UILabel
    {
        public bool IsEnabled { get; protected set; }
        public bool IsVisible { get; set; }

        public float Z { get; set; }

        protected Vector2 position;

        public string Text
        {
            get
            {
                return Writer.Text;
            }
        }

        public TextWriter Writer { get; set; }

        public bool SizeChanged { get; private set; }

        private Vector2 _oldSize;
        private string _oldString;

        public UILabel(Vector2 position)
        {
            this.position = position;
            Writer = new TextWriter((int)position.X, (int)position.Y);
            _oldSize = Writer.WriteAreaSize;
            _oldString = Writer.Text;

            IsVisible = true;
        }

        public void Initialize()
        {
            Writer.Initialize();
        }

        public void Update()
        {
            Writer.Update();

            Vector2 size = Writer.WriteAreaSize;

            if (_oldSize != size || _oldString != Writer.Text)
            {
                SizeChanged = true;
                _oldSize = size;
                _oldString = Writer.Text;
            }
            else
            {
                SizeChanged = false;
            }
        }

        public void Draw()
        {
            if (IsVisible)
            {
                Writer.Draw();
            }
        }

        public void MoveTo(Vector2 target)
        {
            position = target;

            Writer.Move((int)position.X, (int)position.Y);
        }

        public virtual void SetText(string text)
        {
            if (text == Writer.Text)
            {
                return;
            }

            Writer.Text = text;

            int lines = 2 + text.Count(c => TextWriter.LineBreaks.Any(character => character == c));

            Writer.SetWriteArea((int)Writer.GetTextLenght(), Writer.GetLineHeight() * lines);
            Writer.ProgressTextToEnd();
        }
    }
}