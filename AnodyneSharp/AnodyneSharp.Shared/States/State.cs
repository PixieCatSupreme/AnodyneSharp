using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States
{
    public class State
    {
        public bool Exit { get; protected set; } = false;
        public Background Background { get; protected set; }

        public virtual void Create()
        {
        }

        public virtual void Initialize()
        {

        }

        public virtual void Update()
        {
            if (Background != null)
            {
                Background.Update();
            }
        }

        public virtual void Draw()
        {
        }

        public virtual void DrawUI()
        {
        }
    }
}
