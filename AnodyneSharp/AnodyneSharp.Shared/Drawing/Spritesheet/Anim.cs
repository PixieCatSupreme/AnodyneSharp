using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp
{
    public class Anim
    {
        public int Frame => frames[_curIndex];
        public bool Dirty { get; set; }
        public int CurIndex { get { return _curIndex; } }

        public string name;
        public int[] frames;
        public float frameRate;
        public bool looped;
        public float delay;

        private bool _finishedLastFrame;
        private int _curIndex;
        private float _frameTimer;


        public bool Finished => delay == 0 || (!looped && _finishedLastFrame);

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
            _finishedLastFrame = false;

            Dirty = true;
        }

        public void Update()
        {
            if (Finished) return;

            _frameTimer += GameTimes.DeltaTime;
            while (_frameTimer > delay)
            {
                _frameTimer -= delay;
                if (_curIndex == frames.Length - 1)
                {
                    _finishedLastFrame = true;
                    if (looped)
                    {
                        _curIndex = 0;
                    }
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
