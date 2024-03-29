﻿using AnodyneSharp.Entities.Enemy;
using AnodyneSharp.MapData;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
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
        List<Entity> _mapEntities;

        int base_killed_enemies;
        List<Entity> _enemies;

        List<Entity> _keepOnScreen;

        public CollisionGroups(int num_enemies_killed)
        {
            base_killed_enemies = num_enemies_killed;
            _enemies = new List<Entity>();
            _groups = new Dictionary<Type, Group>();
            _mapColliders = new List<Entity>();
            _mapEntities = new List<Entity>();
            _keepOnScreen = new List<Entity>();
        }

        public int KilledEnemies()
        {
            return base_killed_enemies + _enemies.Where(e => !e.exists).Count();
        }

        public void Register(Entity e)
        {
            Type t = e.GetType();
            for(Type d = t; d != typeof(Entity); d = d.BaseType)
            {
                Get(d).targets.Add(e);
                if(Get(d).colliders.Count > 0)
                {
                    e.HasVisibleHitbox = true;
                }
            }

            if(t.IsDefined(typeof(EnemyAttribute),false))
            {
                _enemies.Add(e);
            }

            IEnumerable<CollisionAttribute> cs = t.GetCustomAttributes<CollisionAttribute>();

            if(cs.Any())
            {
                e.HasVisibleHitbox = true;
            }
            
            if (cs.Any(c=>c.MapCollision))
                _mapColliders.Add(e);

            if (cs.Any(c=>c.PartOfMap))
                _mapEntities.Add(e);

            if (cs.Any(c=>c.KeepOnScreen))
                _keepOnScreen.Add(e);

            foreach(Type target in cs.SelectMany(c=>c.Types))
            {
                Get(target).colliders.Add(e);
                if(Get(target).colliders.Count == 1)
                {
                    Get(target).targets.ForEach((e) => e.HasVisibleHitbox = true);
                }
            }
        }

        public void DoCollision(Map map, bool ignore_player_map_collision)
        {   
            foreach(Entity e in _mapColliders.Where(e=>e.exists))
            {
                Touching t = e.allowCollisions;
                if(ignore_player_map_collision && e is Player)
                {
                    e.Solid = false; //during transition no collision with map, but do have tile effects take an effect
                }
                map.Collide(e);
                foreach(Entity m in _mapEntities.Where(m=>m.Solid && m.exists && m.Hitbox.Intersects(e.Hitbox)))
                {
                    m.Collided(e);
                }
                e.allowCollisions = t;
            }
            //map-entity collision sets per-frame state values that are checked in entity-entity collisions(player+dust->raft)
            foreach (Group g in _groups.Values)
            {
                foreach (Entity collider in g.colliders.Where(e => e.exists))
                {
                    foreach (Entity target in g.targets.Where(e => e.exists && !ReferenceEquals(e, collider)))
                    {
                        if (collider.Hitbox.Intersects(target.Hitbox))
                        {
                            collider.Collided(target);
                        }
                    }
                }
            }

            Vector2 roomUpLeft = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
            Vector2 roomBottomRight = roomUpLeft + Vector2.One * GameConstants.SCREEN_WIDTH_IN_PIXELS;

            foreach(Entity e in _keepOnScreen.Where(e=>e.exists))
            {
                if (e.Hitbox.Left < roomUpLeft.X)
                {
                    e.Position.X = roomUpLeft.X;
                    e.touching |= Touching.LEFT;
                }
                else if(e.Hitbox.Right > roomBottomRight.X)
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
