using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp
{
    public static class GameTimes
    {
        public static float FPS { get; private set; }
        public static float TimeScale { get; set; }
        public static float TrueDeltaTime { get; private set; }

        public static float DeltaTime
        {
            get
            {
                return TrueDeltaTime * TimeScale;
            }
        }

        private static Queue<float> _fpsQueue;
        private static int _maxSamples = 100;

        static GameTimes()
        {
            _fpsQueue = new Queue<float>(_maxSamples);

            TimeScale = 1;
        }

        public static void UpdateTimes(GameTime gameTime)
        {
            TrueDeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public static void UpdateFPS(GameTime gameTime)
        {
            if (_fpsQueue.Count > _maxSamples)
            {
                _fpsQueue.Dequeue();
                FPS = _fpsQueue.Average(i => i);
            }
            _fpsQueue.Enqueue( 1f / (float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
