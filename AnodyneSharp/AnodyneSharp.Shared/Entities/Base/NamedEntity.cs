using AnodyneSharp.Registry;
using System;
using System.Linq;

namespace AnodyneSharp.Entities
{
    [AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class NamedEntity : Attribute
    {
        readonly string xmlName;
        public readonly string type;
        public readonly string map;
        public readonly int[] frames;

        public NamedEntity() : this(null) { }

        public NamedEntity(string xmlName)
            : this(xmlName, type:null)
        { }

        public NamedEntity(string xmlName = null, string type = null, string map = null, params int[] frames)
        {
            this.xmlName = xmlName;
            this.type = type;
            this.frames = frames;
            this.map = map;
        }

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

        public bool Matches(int f, string t, string m)
        {
            return (map ?? m) == m && (type ?? t) == t && (frames.Length == 0 || frames.Contains(f));
        }
    }
}
