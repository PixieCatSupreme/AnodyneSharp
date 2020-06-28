using AnodyneSharp.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnodyneSharp.Entities
{
    //Keeps track of what needs to collide with what, and calls appropriate 
    public class CollisionGroups
    {
        private class Group
        {
            public Group()
            {
                targets = new List<Entity>();
                colliders = new List<Entity>();
            }
            public List<Entity> targets;
            public List<Entity> colliders;
        }
        Dictionary<Type, Group> _groups;
        List<Entity> _mapColliders;

        public CollisionGroups()
        {
            _groups = new Dictionary<Type, Group>();
            _mapColliders = new List<Entity>();
        }

        public void Register(Entity e)
        {
            Type t = e.GetType();
            Get(t).targets.Add(e);
            CollisionAttribute c = t.GetCustomAttribute<CollisionAttribute>();

            if (c == null)
            {
                return;
            }

            if (c.MapCollision)
                _mapColliders.Add(e);
            foreach(Type target in c.Types)
            {
                Get(target).colliders.Add(e);
            }
        }

        public void DoCollision(TileMap bg, TileMap bg2)
        {
            foreach(Entity e in _mapColliders.Where(e=>e.exists))
            {
                bg.Collide(e);
                bg2.Collide(e);
            }
            foreach(Group g in _groups.Values)
            {
                foreach(Entity collider in g.colliders.Where(e => e.exists))
                {
                    foreach(Entity target in g.targets.Where(e => e.exists))
                    {
                        if(collider.Hitbox.Intersects(target.Hitbox))
                        {
                            collider.Collided(target);
                        }
                    }
                }
            }
        }

        private Group Get(Type t)
        {
            if(!_groups.TryGetValue(t, out Group g))
            {
                g = new Group();
                _groups.Add(t, g);
            }
            return g;
        }
    }
}
