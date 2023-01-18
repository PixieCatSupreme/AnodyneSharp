using AnodyneSharp.Dialogue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnodyneSharp.Registry
{
    public enum Resolution
    {
        Windowed,
        Scaled,
        Stretch
    }

    public enum FPS
    {
        Fixed,
        VSync,
        Unlocked
    }

    public class Settings
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Language language { get; set; } = Language.EN;
        
        public float music_volume_scale { get; set; } = 1.0f;
        public float sfx_volume_scale { get; set; } = 1.0f;
        
        public bool autosave_on { get; set; } = true;

        public bool pause_on_unfocus { get; set; } = true;
        public bool fast_text { get; set; } = false;
        public bool invincible { get; set; } = false;
        public bool extended_coyote { get; set; } = false;
        public bool guaranteed_health { get; set; } = false;
        
        public Resolution resolution { get; set; } = Resolution.Windowed;
        public int scale { get; set; } = 3;

        public FPS fps { get; set; } = FPS.Fixed;

        public float flash_brightness { get; set; } = 1.0f;
        public float flash_easing { get; set; } = 0.0f;
        public bool screenshake { get; set; } = true;

        public static Settings Load()
        {
            try
            {
                string save = File.ReadAllText($"{GameConstants.SavePath}Settings.json");
                return JsonSerializer.Deserialize<Settings>(save);
            }
            catch
            {
                return new();
            }
        }

        public void Save()
        {
            File.WriteAllText($"{GameConstants.SavePath}Settings.json", JsonSerializer.Serialize<Settings>(this));
        }
    }
}
