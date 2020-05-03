using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Cheatz
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class CheatAttribute : Attribute
    {
        public readonly string cheatInput;

        public CheatAttribute(string inputString)
        {
            cheatInput = inputString;
        }
    }
}
