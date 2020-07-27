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
            public float max;
            public string name;
        }
        SortedDictionary<float,Timer> timers = new SortedDictionary<float, Timer>();

        private float current = 0.0f;

        public void Reset()
        {
            timers = new SortedDictionary<float, Timer>();
        }

        public void AddTimer(float time, string name)
        {
            timers.Add(current+time,new Timer() { max=time, name=name });
        }

        public override void Update(float deltaTime)
        {
            current += deltaTime;

            if (timers.Count > 0)
            {
                var min = timers.First();

                while (min.Key <= current)
                {
                    TriggerEvent(min.Value.name);
                    timers.Remove(min.Key);
                    AddTimer(min.Value.max, min.Value.name);
                    min = timers.First();
                }
            }
            base.Update(deltaTime);
        }
    }
}
