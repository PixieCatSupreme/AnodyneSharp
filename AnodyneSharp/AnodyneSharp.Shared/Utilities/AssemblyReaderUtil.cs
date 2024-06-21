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
        public static Stream? GetStream(string path, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetEntryAssembly();

            return assembly?.GetManifestResourceStream($"{assembly.GetName().Name}.{path}") ?? null;
        }
    }
#nullable restore
}
