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
            bgm.SetVolume(volume * GlobalState.music_volume_scale);
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

        public static bool PlaySoundEffect(params string[] names)
        {
            string name = names[GlobalState.RNG.Next(0, names.Length)];
            return CreateSoundInstance(name);
        }

        public static bool PlayPitchedSoundEffect(string name, float pitch, float volume = 1)
        {
            return CreateSoundInstance(name, volume, pitch);
        }

        private static bool CreateSoundInstance(string name, float volume = 1, float pitch = 0)
        {
            SoundEffect sfx = ResourceManager.GetSFX(name);
            if (sfx != null)
            {
                SoundEffectInstance instance = sfx.CreateInstance();

                instance.Pitch = pitch;
                instance.Volume = volume * GlobalState.sfx_volume_scale;
                instance.Play();
            }
            return false;
        }
    }
}