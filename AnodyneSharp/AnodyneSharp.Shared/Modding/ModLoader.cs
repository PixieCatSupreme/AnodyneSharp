#nullable enable

using AnodyneSharp.Utilities;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace AnodyneSharp.Modding
{
    public static class ModLoader
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
            var loadedAssemblies = assemblies.Select(s => loadContext.LoadFromAssemblyPath(s)).ToList();
            return loadedAssemblies.SelectMany(assembly=>assembly.GetTypes())
                .Where(t=>typeof(IMod).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t=>(IMod?)Activator.CreateInstance(t))
                .NotNull();
        }

        private static IEnumerable<string> DLLFiles()
        {
            Matcher matcher = new();
            matcher.AddInclude("Mods/*/Assemblies/*.dll");

            string searchDir = Path.GetDirectoryName(AppContext.BaseDirectory)!;

            return matcher.GetResultsInFullPath(searchDir);
        }
    }
}
