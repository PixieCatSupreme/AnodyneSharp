﻿using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.States
{
    public class State
    {
        public bool Exit { get; set; } = false;
        public bool UpdateEntities { get; protected set; } = true;
        public bool DrawPlayState { get; protected set; } = true;

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
