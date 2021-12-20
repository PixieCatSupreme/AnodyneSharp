using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AnodyneSharp.MapData.Settings
{
#nullable enable
    public class SettingGroup
    {
        public string? Music { get; set; }
        public string? Darkness { get; set; }
        public float? DarknessAlpha { get; set; }
        public string? ReplaceTiles { get; set; }
    }

    public record MapRegion(int X, int Y, int Width, int Height, SettingGroup Settings);

    public record MapEvent(string Event, SettingGroup Settings);

    public record MapSettings([property: JsonPropertyName("Settings")] SettingGroup Default, List<MapRegion> Areas, List<MapEvent> Events)
    {
        public T Get<T>(Func<SettingGroup, T?> getter, Vector2 location, T defaultValue) where T : struct
        {
            T? current = default;

            foreach(var area in Areas)
            {
                if(new Rectangle(area.X,area.Y,area.Width,area.Height).Contains(location))
                {
                    current ??= getter(area.Settings);
                }
            }

            foreach(var eventCheck in Events)
            {
                if(GlobalState.events.GetEvent(eventCheck.Event) != 0)
                {
                    current ??= getter(eventCheck.Settings);
                }
            }

            current ??= getter(Default);

            return current ?? defaultValue;
        }

        public T Get<T>(Func<SettingGroup, T?> getter, Vector2 location, T defaultValue) where T : class
        {
            T? current = default;

            foreach (var area in Areas)
            {
                if (new Rectangle(area.X, area.Y, area.Width, area.Height).Contains(location))
                {
                    current ??= getter(area.Settings);
                }
            }

            foreach (var eventCheck in Events)
            {
                if (GlobalState.events.GetEvent(eventCheck.Event) != 0)
                {
                    current ??= getter(eventCheck.Settings);
                }
            }

            current ??= getter(Default);

            return current ?? defaultValue;
        }
    };
#nullable restore
}
