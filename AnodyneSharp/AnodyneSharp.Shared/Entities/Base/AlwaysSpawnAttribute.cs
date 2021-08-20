using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    //Ignore EntityPreset::Alive for spawning, it is used for internal logic of the entity
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed class AlwaysSpawnAttribute : Attribute
    {
        public AlwaysSpawnAttribute()
        {
        }
    }
}
