using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Sounds
{
    public static class SoundManager
    {
        public static string CurrentSongName { get; private set; }

        public static bool IsPlayingSong
        {
            get
            {
                return CurrentSongName != "";
            }
        }

        private static SongPlayer bgm = new();

        static SoundManager()
        {
            CurrentSongName = "";
        }

        public static bool PlaySong(string name, float volume = 1f, bool isRepeating = true)
        {
            string song = ResourceManager.GetMusicPath(name);
            if (song != null)
            {
                CurrentSongName = name;
                SetSongVolume(volume);
                bgm.Play(song);
                return true;
            }
            else
            {
                StopSong();
                return false;
            }
        }

        public static void SetSongVolume(float volume)
        {
            bgm.SetVolume(volume * GlobalState.settings.music_volume_scale);
        }

        public static float GetVolume()
        {
            return bgm.GetVolume() / GlobalState.settings.music_volume_scale;
        }

        public static void SetSongVolume() => SetSongVolume(1f);

        public static bool StopSong()
        {
            if (IsPlayingSong)
            {
                bgm.Stop();
                CurrentSongName = "";
                return true;
            }
            return false;
        }

        public static void PlaySoundEffect(params string[] names)
        {
            var sfx = names.Select((n) => ResourceManager.GetSFX(n)).Where((n) => n != null).ToArray();
            if (sfx.Length == 0) return;

            CreateSoundInstance(sfx[GlobalState.RNG.Next(0,sfx.Length)]);
        }

        public static void PlayPitchedSoundEffect(string name, float pitch, float volume = 1)
        {
            CreateSoundInstance(ResourceManager.GetSFX(name), volume, pitch);
        }

        private static void CreateSoundInstance(SoundEffectInstance sfx, float volume = 1, float pitch = 0)
        {
            if (sfx != null)
            {
                sfx.Pitch = pitch;
                sfx.Volume = volume * GlobalState.settings.sfx_volume_scale;
                sfx.Play();
            }
        }
    }
}