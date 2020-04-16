using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class CollisionAttribute : Attribute
    {
        public CollisionAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; }

        public bool MapCollision { get; set; }
    }
}
