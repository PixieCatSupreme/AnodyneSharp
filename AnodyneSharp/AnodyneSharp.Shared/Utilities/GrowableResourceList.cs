using System;
using System.Collections.Generic;

namespace AnodyneSharp.Utilities
{
    public class GrowableResourceList<T>
    {
        Func<T> spawner;
        List<T> values = new();
        int cur_index = 0;
        public GrowableResourceList(Func<T> spawner)
        {
            this.spawner = spawner;
        }

        public T GetNext()
        {
            if(values.Count == cur_index)
            {
                values.Add(spawner());
            }
            return values[cur_index++];
        }

        public void Reset()
        {
            cur_index = 0;
        }
    }
}
