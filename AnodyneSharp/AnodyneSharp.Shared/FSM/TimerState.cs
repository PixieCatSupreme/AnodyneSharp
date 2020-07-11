using RSG;
using System.Collections.Generic;

namespace AnodyneSharp.FSM
{
    public class TimerState : AbstractState
    {
        private class Timer
        {
            public float current = 0.0f;
            public float max;
            public string name;
        }
        List<Timer> timers = new List<Timer>();

        public void Reset()
        {
            timers = new List<Timer>();
        }

        public void AddTimer(float time, string name)
        {
            timers.Add(new Timer() { max=time, name=name });
        }

        public void DoTimers(float deltaTime)
        {
            foreach(Timer t in timers)
            {
                t.current += deltaTime;
                if(t.current >= t.max)
                {
                    t.current = 0.0f;
                    TriggerEvent(t.name);
                }
            }
        }
    }
}
