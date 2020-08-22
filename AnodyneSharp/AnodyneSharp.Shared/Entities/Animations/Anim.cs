using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Animations
{
    public class Anim
    {
        public int Frame => frames[_curIndex];
        public bool Dirty { get; set; }

        public string name;
        public int[] frames;
        public float frameRate;
        public bool looped;
        public float delay;

        private int _curIndex;
        private float _frameTimer;


        public bool finished;

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
            Dirty = true;
        }

        public override string ToString()
        {
            return name;
        }

        public void Reset()
        {
            _curIndex = 0;
            _frameTimer = 0;

            if (delay <= 0)
            {
                finished = true;
            }
            else
            {
                finished = false;
            }
            Dirty = true;
        }

        public void Update()
        {
            _frameTimer += GameTimes.DeltaTime;
            while (_frameTimer > delay)
            {
                _frameTimer -= delay;
                if (_curIndex == frames.Length - 1)
                {
                    if (looped)
                    {
                        _curIndex = 0;
                    }

                    finished = true;
                }
                else
                {
                    _curIndex++;
                }

                Dirty = true;
            }
        }
    }
}
