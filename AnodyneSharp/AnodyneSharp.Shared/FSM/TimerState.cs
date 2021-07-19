using Microsoft.Xna.Framework;
using RSG;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace AnodyneSharp.FSM
{
    public class TimerState : AbstractState
    {
        private class Timer
        {
            public float current;
            public float max;
            public string name;
        }
        List<Timer> timers = new();

        private float current = 0.0f;

        public void Reset()
        {
            timers = new();
        }

        public void Advance(float time)
        {
            current += time;
        }

        public void AddTimer(float time, string name)
        {
            timers.Add(new() { current = current + time, max = time, name = name });
            timers.Sort((t1, t2) => t1.current.CompareTo(t2.current));
        }

        public override void Update(float deltaTime)
        {
            current += deltaTime;

            if (timers.Count > 0)
            {
                var min = timers.First();

                while (min.current <= current)
                {
                    TriggerEvent(min.name);
                    timers.RemoveAt(0);
                    AddTimer(min.max, min.name);
                    min = timers.First();
                }
            }
            base.Update(deltaTime);
        }
    }
}
