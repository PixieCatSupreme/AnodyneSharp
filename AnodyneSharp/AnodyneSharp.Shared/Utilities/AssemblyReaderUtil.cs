using AnodyneSharp.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            path = $"{assembly!.GetName().Name}.{path}";

            Stream? s = assembly.GetManifestResourceStream(path) ?? null;

            return ModLoader.mods.Aggregate(s,(stream,mod) => mod.OnManifestLoad(stream,path));
        }
    }
#nullable restore
}
