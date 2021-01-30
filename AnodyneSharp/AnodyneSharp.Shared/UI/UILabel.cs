﻿using AnodyneSharp.Drawing;
using AnodyneSharp.Resources;
using AnodyneSharp.UI.Font;
using AnodyneSharp.UI.Text;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnodyneSharp.UI
{
    public class UILabel
    {
        public bool IsEnabled { get; protected set; }
        public bool IsVisible { get; set; }

        public float Z { get; set; }

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;

                Rectangle r = Writer.WriteArea;

                Writer.SetWriteArea((int)_position.X, (int)_position.Y, r.Width, r.Height);
            }
        }

        public string Text
        {
            get
            {
                return Writer.Text;
            }
        }

        public TextWriter Writer { get; set; }

        public bool SizeChanged { get; private set; }
        public float Opacity
        {
            get
            {
                return Writer.Opacity;
            }
            set
            {
                Writer.Opacity = value;
            }
        }

        private Vector2 _oldSize;
        private string _oldString;

        private Color _color;
        private bool _drawShadow;

        private Vector2 _position;

        public UILabel(Vector2 position, bool drawShadow, Color? color = null, DrawOrder layer = DrawOrder.MENUTEXT)
        {
            _position = position;

            Writer = new TextWriter((int)position.X, (int)position.Y)
            {
                drawLayer = layer
            };

            _oldSize = Writer.WriteAreaSize;
            _oldString = Writer.Text;

            IsVisible = true;

            _color = color ?? Color.White;
            _drawShadow = drawShadow;
        }


        public void Initialize(bool forceEnglish = false)
        {
            Writer.SetSpriteFont(FontManager.InitFont(_color, forceEnglish), ResourceManager.GetTexture("consoleButtons"));
            Writer.IgnoreSoftLineBreaks = true;
            Writer.DrawShadow = _drawShadow;
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

        public virtual void SetText(string text)
        {
            if (text == Writer.Text)
            {
                return;
            }

            Writer.Text = text;

            int lines = 2 + Writer.Text.Count(c => TextWriter.LineBreaks.Any(character => character == c));

            Writer.SetWriteArea((int)Writer.GetTextLength(), Writer.GetLineHeight() * lines);
            Writer.ProgressTextToEnd();
        }
    }
}