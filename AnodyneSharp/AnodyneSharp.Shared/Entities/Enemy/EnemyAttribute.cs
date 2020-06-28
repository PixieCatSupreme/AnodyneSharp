using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy
{
    [AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class EnemyAttribute : Attribute
    {
        public EnemyAttribute()
        {
        }
    }
}
