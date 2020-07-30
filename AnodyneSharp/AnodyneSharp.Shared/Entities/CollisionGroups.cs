using AnodyneSharp.Entities.Enemy;
using AnodyneSharp.Map;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
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

        int base_killed_enemies;
        List<Entity> _enemies;

        List<Entity> _keepOnScreen;

        public CollisionGroups(int num_enemies_killed)
        {
            base_killed_enemies = num_enemies_killed;
            _enemies = new List<Entity>();
            _groups = new Dictionary<Type, Group>();
            _mapColliders = new List<Entity>();
            _keepOnScreen = new List<Entity>();
        }

        public int KilledEnemies()
        {
            return base_killed_enemies + _enemies.Where(e => !e.exists).Count();
        }

        public void Register(Entity e)
        {
            Type t = e.GetType();
            Get(t).targets.Add(e);

            if(t.IsDefined(typeof(EnemyAttribute),false))
            {
                _enemies.Add(e);
            }

            CollisionAttribute c = t.GetCustomAttribute<CollisionAttribute>();

            if (c == null)
            {
                return;
            }

            if (c.MapCollision)
                _mapColliders.Add(e);

            if (c.KeepOnScreen)
                _keepOnScreen.Add(e);

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

            Vector2 roomUpLeft = GlobalState.CurrentMapGrid * GameConstants.SCREEN_WIDTH_IN_PIXELS;
            Vector2 roomBottomRight = roomUpLeft + Vector2.One * GameConstants.SCREEN_WIDTH_IN_PIXELS;

            foreach(Entity e in _keepOnScreen.Where(e=>e.exists))
            {
                if (e.Hitbox.Right < roomUpLeft.X)
                {
                    e.Position.X = roomUpLeft.X;
                    e.touching |= Touching.LEFT;
                }
                else if(e.Hitbox.Left > roomBottomRight.X)
                {
                    e.Position.X = roomBottomRight.X - e.width;
                    e.touching |= Touching.RIGHT;
                }

                if(e.Hitbox.Top < roomUpLeft.Y)
                {
                    e.Position.Y = roomUpLeft.Y;
                    e.touching |= Touching.UP;
                }
                else if(e.Hitbox.Bottom > roomBottomRight.Y)
                {
                    e.Position.Y = roomBottomRight.Y - e.height;
                    e.touching |= Touching.DOWN;
                }
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
