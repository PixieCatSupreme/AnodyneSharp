using System;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Entities
{
    class EntityPool<T> where T : Entity
    {
        private List<T> _pool;

        //View for collision registration
        public IEnumerable<Entity> Entities => _pool;


        public int Alive => _pool.Where(t => t.exists).Count();

        public EntityPool(int total, Func<T> create)
        {
            _pool = Enumerable.Repeat(create, total).Select(c => { T e = c(); e.exists = false; return e; }).ToList();
        }

        public void Spawn(Action<T> onSpawn = null, int total = 1)
        {
            if (onSpawn == null)
                onSpawn = t => { };

            foreach(T t in _pool.Where(t=>!t.exists).Take(total))
            {
                t.exists = true;
                onSpawn(t);
            }
        }

    }
}
