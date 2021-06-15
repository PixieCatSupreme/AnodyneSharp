using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.GameEvents
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class EventsAttribute : Attribute
    {
        public Type[] Types { get; }

        // This is a positional argument
        public EventsAttribute(params Type[] eventTypes)
        {
            Types = eventTypes;
        }
    }
}
