using System;
using System.Linq;

namespace AnodyneSharp.Entities
{
    [AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class NamedEntity : Attribute
    {
        readonly string xmlName;
        public readonly string type;
        public readonly int[] frames;

        public NamedEntity(string xmlName = null, string type = null, params int[] frames)
        {
            this.xmlName = xmlName;
            this.type = type;
            this.frames = frames;
        }

        public string GetName(Type type)
        {
            if(xmlName != null)
            {
                return xmlName;
            } else
            {
                return type.Name;
            }
        }

        public bool Matches(int f, string t)
        {
            return (type ?? t) == t && (frames.Length == 0 || frames.Contains(f));
        }
    }
}
