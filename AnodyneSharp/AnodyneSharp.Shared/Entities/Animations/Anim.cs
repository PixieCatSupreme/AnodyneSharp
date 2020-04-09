using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Animations
{
    public class Anim
    {
        public string name;
        public int[] frames;
        public float frameRate;
        public bool looped;
        public float delay;

        public Anim(string name, int[] frames, float frameRate, bool looped = true)
        {
            this.name = name;
            this.frames = frames;
            this.frameRate = frameRate;

            if (frameRate > 0)
            {
                delay = 1.0f / frameRate;
            }

            this.looped = looped;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
