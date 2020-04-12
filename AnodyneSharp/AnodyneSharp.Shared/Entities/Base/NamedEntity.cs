using System;

namespace AnodyneSharp.Entities
{
    [AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class NamedEntity : Attribute
    {
        readonly string xmlName;

        public NamedEntity(string xmlName = null)
        {
            this.xmlName = xmlName;
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
    }
}
