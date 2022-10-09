using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace AnodyneSharp.MapData.Settings
{
#nullable enable
    public class SettingGroup
    {
        public string? Music { get; set; }
        public float? MusicVolume { get; set; }
        public string? Ambience { get; set; }
        public float? AmbienceVolume { get; set; }
        public string? Darkness { get; set; }
        public float? DarknessAlpha { get; set; }
        public string? FG_Blend { get; set; }
        public string? ExtraBlend { get; set; }
        public string? ReplaceTiles { get; set; }
        public bool? Static { get; set; }
        public bool? PlayerLight { get; set; }

        public bool? BOI { get; set; }
    }

    public class TileReplacement
    {
        public Dictionary<int, int> Replacements { get; init; } = new();

        public TileReplacement(string value)
        {
            string[] replace_s = value.Split(';');
            foreach(string r in replace_s.Where(s=>s.Length > 0))
            {
                string[] rule = r.Split('-');
                Replacements[int.Parse(rule[0])] = int.Parse(rule[1]);
            }
        }
    }

    public record MapRegion(int X, int Y, int Width, int Height, SettingGroup Settings);

    public record MapEvent(string Event, SettingGroup Settings);

    public record MapSettings([property: JsonPropertyName("Settings")] SettingGroup Default, List<MapRegion> Areas, List<MapEvent> Events)
    {
        public List<SettingGroup> GetSettingPriorities(Vector2 location)
        {
            List<SettingGroup> groups = new();
            foreach (var area in Areas)
            {
                if (new Rectangle(area.X, area.Y, area.Width, area.Height).Contains(location))
                {
                    groups.Add(area.Settings);
                }
            }

            foreach (var eventCheck in Events)
            {
                if (GlobalState.events.GetEvent(eventCheck.Event) != 0)
                {
                    groups.Add(eventCheck.Settings);
                }
            }

            groups.Add(Default);

            return groups;
        }

        public static T Get<T>(Func<SettingGroup, T?> getter, List<SettingGroup> priorities, T defaultValue) where T : struct
        {
            return priorities.Select(getter).Where(n => n.HasValue).FirstOrDefault() ?? defaultValue;
        }

        public static T Get<T>(Func<SettingGroup, T?> getter, List<SettingGroup> priorities, T defaultValue) where T : class
        {
            return priorities.Select(getter).Where(n => n != null).FirstOrDefault() ?? defaultValue;
        }
    };
#nullable restore
}
