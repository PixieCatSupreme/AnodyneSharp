using AnodyneSharp.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.FSM
{
    public class CollisionEvent<E> : EventArgs
    {
        public E entity;
    }
}
