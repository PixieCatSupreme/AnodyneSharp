#nullable enable

using AnodyneSharp.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace AnodyneSharp.Modding
{
    internal static class ModLoader
    {
        public static List<IMod> mods = new();

        public static void Initialize()
        {
            mods.AddRange(LoadMods());
        }

        private static IEnumerable<IMod> LoadMods()
        {
            AssemblyLoadContext loadContext = AssemblyLoadContext.Default;
            var assemblies = DLLFiles().ToList();
            return assemblies.Select(s => loadContext.LoadFromAssemblyPath(s)).SelectMany(assembly=>assembly.GetTypes())
                .Where(t=>typeof(IMod).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t=>(IMod?)Activator.CreateInstance(t))
                .NotNull();
        }

        private static IEnumerable<string> DLLFiles()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)!, "Mods");
            if(!Directory.Exists(path))
            {
                return Enumerable.Empty<string>();
            }
            var paths = Directory.GetDirectories(path).SelectMany(p => Directory.GetFiles(p, "Assemblies/*.dll", System.IO.SearchOption.AllDirectories));

            return paths;
        }
    }
}
