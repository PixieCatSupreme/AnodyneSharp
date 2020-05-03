using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    class Parabola_Thing
    {
        private float t = 0;
        private float height;
        private float period;
        private Entity entity;

        public Parabola_Thing(Entity entity, float height, float period)
        {
            this.entity = entity;
            this.height = height;
            this.period = period;
        }

        public bool Tick()
        {
            t += GameTimes.DeltaTime;
            
            if (t > period) return true;

            float half_period = period / 2;

            entity.offset.Y = t * (period - t) / (half_period*half_period) * height;

            return false;
        }

        public void ResetTime()
        {
            t = 0;
        }
    }
}
