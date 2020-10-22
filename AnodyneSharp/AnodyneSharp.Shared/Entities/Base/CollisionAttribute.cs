using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed class CollisionAttribute : Attribute
    {
        public CollisionAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; }

        public bool PartOfMap { get; set; }

        public bool MapCollision { get; set; }

        public bool KeepOnScreen { get; set; }
    }
}
