using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnodyneSharp.MapData
{
    public class SwapperControl
    {
        public enum State
        {
            Default,
            Allow,
            Disallow,
            DisallowSilently
        }

        struct Region
        {
            public State allow;
            public Rectangle area;
        }

        List<Region> regions = new();

        public SwapperControl(string mapName)
        {
            var assembly = Assembly.GetEntryAssembly();

            string path = $"{assembly.GetName().Name}.Content.Maps.{mapName}.Swapper.dat";

            using Stream stream = assembly.GetManifestResourceStream(path);

            if (stream == null)
            {
                return;
            }

            using StreamReader reader = new(stream);

            while(!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split('\t');
                if(line.Length == 5)
                {
                    ArraySegment<string> rectString = new(line, 1, 4);
                    int[] rect = rectString.Select((s) => (int)float.Parse(s)).ToArray();
                    regions.Add(new() { allow = Enum.Parse<State>(line[0]), area = new(rect[0], rect[1], rect[2], rect[3]) });
                }
            }
        }

        public State CheckCoord(Vector2 coord)
        {
            return regions.Where(r => r.area.Contains(coord)).Select(r => r.allow).FirstOrDefault();
        }
    }
}