using AnodyneSharp.Entities;
using AnodyneSharp.GameEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnodyneSharp
{
    public class EntityEventRegistry
    {
        Dictionary<Type, List<Entity>> listeners = new();

        public void Register(Entity e)
        {
            Type t = e.GetType();
            EventsAttribute events = t.GetCustomAttribute<EventsAttribute>();
            if(events == null)
            {
                return;
            }

            foreach(Type eventType in events.Types)
            {
                if(!listeners.TryGetValue(eventType, out List<Entity> entities)) {
                    entities = new();
                    listeners.Add(eventType, entities);
                }
                entities.Add(e);
            }
        }

        public void FireEvent(GameEvent ev)
        {
            if(listeners.TryGetValue(ev.GetType(),out List<Entity> entities))
            {
                foreach(var e in entities.Where(e=>e.exists))
                {
                    e.OnEvent(ev);
                }
            }
        }
    }
}
