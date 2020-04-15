using AnodyneSharp.Map;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    //Keeps track of what needs to collide with what, and calls appropriate 
    public class CollisionGroups
    {
        private class Group
        {
            public List<Entity> targets;
            public List<Entity> colliders;
        }
        Dictionary<Type, Group> _groups;
        List<Entity> _mapColliders;

        public void DoCollision(TileMap bg, TileMap bg2)
        {
            foreach(Entity e in _mapColliders)
            {
                bg.Collide(e);
                bg2.Collide(e);
            }
            foreach(Group g in _groups.Values)
            {
                foreach(Entity collider in g.colliders)
                {
                    foreach(Entity target in g.targets)
                    {
                        if(collider.Hitbox.Intersects(target.Hitbox))
                        {
                            collider.Collided(target);
                        }
                    }
                }
            }
        }
    }
}
