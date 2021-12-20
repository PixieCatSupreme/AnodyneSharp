using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AnodyneSharp.Utilities
{
#nullable enable
    public static class AssemblyReaderUtil
    {
        public static Stream? GetStream(string path)
        {
            Assembly asm = Assembly.GetCallingAssembly();

            return asm.GetManifestResourceStream($"{asm.GetName().Name}.{path}");
        }
    }
#nullable restore
}
