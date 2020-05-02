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
        public static Song CurrentSong { get; private set; }
        public static string CurrentSongName { get; private set; }

        public static bool IsPlayingSong
        {
            get
            {
                return CurrentSong != null;
            }
        }

        public static float Volume
        {
            get
            {
                return MediaPlayer.Volume;
            }
        }


        private static AudioListener _listener;

        static SoundManager()
        {

            CurrentSongName = "";
        }

        public static bool PlaySong(string name, float volume = 1f, bool isRepeating = true)
        {
            Song song = ResourceManager.GetMusic(name);
            if (song != null)
            {
                MediaPlayer.Volume = volume * GlobalState.volume_scale;
                MediaPlayer.IsRepeating = isRepeating;
                MediaPlayer.Play(song);
                CurrentSong = song;
                CurrentSongName = name;

                return true;
            }
            else
            {
                StopSong();
                return false;
            }

        }

        public static bool PauseSong()
        {
            if (CurrentSong != null)
            {
                MediaPlayer.Pause();
                return true;
            }
            return false;
        }

        public static bool ResumeSong()
        {
            if (CurrentSong != null)
            {
                MediaPlayer.Resume();
                return true;
            }
            return false;
        }

        public static bool SetSongVolume(float volume)
        {
            if (CurrentSong != null)
            {
                MediaPlayer.Volume = volume;
                return true;
            }
            return false;
        }

        public static bool SetSongVolume()
        {
            if (CurrentSong != null)
            {
                MediaPlayer.Volume *= GlobalState.volume_scale;
                return true;
            }
            return false;
        }

        public static bool StopSong()
        {
            if (CurrentSong != null)
            {
                MediaPlayer.Stop();
                CurrentSong = null;
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
                instance.Volume = volume * GlobalState.volume_scale;
                instance.Play();
            }
            return false;
        }
    }
}