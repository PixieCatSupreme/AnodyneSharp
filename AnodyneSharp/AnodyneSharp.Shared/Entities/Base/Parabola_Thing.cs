using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    public class Parabola_Thing
    {
        private float t = 0;
        private float height;
        private float period;
        private float start;
        private Entity entity;

        public Parabola_Thing(Entity entity, float height, float period)
        {
            this.entity = entity;
            this.start = entity.offset.Y;
            this.height = height;
            this.period = period;
        }

        public bool Tick()
        {
            t += GameTimes.DeltaTime;

            if (t > period)
            {
                entity.offset.Y = start;
                if (entity.shadow != null) entity.shadow.UpdateShadow(0f);
                return true;
            }

            float half_period = period / 2;

            entity.offset.Y = start + t * (period - t) / (half_period*half_period) * height;

            if(entity.shadow != null)
            {
                entity.shadow.UpdateShadow(MathF.Abs(0.5f - Progress())*2);
            }

            return false;
        }
        
        public float Progress()
        {
            return MathF.Min(t / period, 1f);
        }

        public void ResetTime()
        {
            t = 0;
        }
    }
}
