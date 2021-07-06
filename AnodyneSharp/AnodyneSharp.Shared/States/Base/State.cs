using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States
{
    public delegate void ChangeState(AnodyneGame.GameState state);

    public class State
    {
        public bool Exit { get; protected set; } = false;
        public bool UpdateEntities { get; protected set; } = true;
        public bool DrawPlayState { get; protected set; } = true;

        public ChangeState ChangeStateEvent;

        public virtual void Create()
        {
        }

        public virtual void Initialize()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void Draw()
        {
        }

        public virtual void DrawUI()
        {
        }
    }
}
