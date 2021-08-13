extern alias MyVorbis;

using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using MyVorbis::NVorbis;
using System.Linq;

namespace AnodyneSharp.Sounds
{
    internal class SongPlayer
    {
        const int BufferMs = 128;
        
        DynamicSoundEffectInstance player = new(44100,AudioChannels.Stereo);
        VorbisReader reader;

        float[] vorbis_samples = new float[SoundEffect.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(BufferMs),44100,AudioChannels.Stereo)];

        public SongPlayer()
        {
            player.BufferNeeded += BufferNeeded;
        }

        private void BufferNeeded(object sender, EventArgs e)
        {
            if (reader.IsEndOfStream)
            {
                int.TryParse(reader.Tags.GetTagSingle("LOOPSTART"), out int startLoop);
                reader.SeekTo(startLoop);
            }
            if (!reader.IsEndOfStream)
            {
                int total = reader.ReadSamples(vorbis_samples, 0, vorbis_samples.Length);

                byte[] res = new byte[total * 2];
                for(int i = 0; i < total; ++i)
                {
                    int tmp = (int)(short.MaxValue * vorbis_samples[i]);
                    Math.Clamp(tmp, short.MinValue, short.MaxValue);
                    short val = (short)tmp;
                    res[i * 2] = (byte)(val & 0xFF);
                    res[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
                }
                player.SubmitBuffer(res);
            }
        }

        internal float GetVolume()
        {
            return player.Volume;
        }

        public void Play(string song)
        {
            if (reader != null) reader.Dispose();
            player.Stop();
            reader = new VorbisReader(song);
            BufferNeeded(null, null);
            if(player.State != SoundState.Playing)
            {
                player.Play();
            }
        }

        public void Stop() => player.Stop();

        public void SetVolume(float v) => player.Volume = v;

    }
}
