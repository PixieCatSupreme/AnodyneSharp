using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AnodyneSharp.Entities
{
    public enum Permanence
    {
        GRID_LOCAL,
        MAP_LOCAL,
        GLOBAL
    }

    //Decoupled from EntityPreset, saved separately so saving only this info for each entity is possible
    public class EntityState
    {
        public bool Alive { get; set; } = true;
        public bool Activated { get; set; } = false; //State-keeping for entities that need it beyond getting killed
    }

    public class EntityPreset
    {

        public Point GridPosition
        {
            get
            {
                return MapUtilities.GetRoomCoordinate(Position);
            }
        }

        public Type Type { get; private set; }
        public Vector2 Position { get; private set; }
        public Guid EntityID { get; private set; }
        public int Frame { get; private set; }
        public Permanence Permanence { get; private set; }
        public string TypeValue { get; private set; }
        public bool Alive
        {
            get
            {
                return EntityManager.State.GetValueOrDefault(EntityID)?.Alive ?? true;
            }
            set
            {
                EntityState s = EntityManager.State.GetValueOrDefault(EntityID);
                if (value && s != null)
                {
                    s.Alive = true;
                    if (s.Activated == false)
                    {
                        EntityManager.State.Remove(EntityID);
                    }
                }
                else if (!value)
                {
                    if (s != null)
                    {
                        s.Alive = false;
                    }
                    else
                    {
                        EntityManager.State.Add(EntityID, new() { Alive = false });
                    }
                }
            }
        }
        public bool Activated
        {
            get
            {
                return EntityManager.State.GetValueOrDefault(EntityID)?.Activated ?? false;
            }
            set
            {
                EntityState s = EntityManager.State.GetValueOrDefault(EntityID);
                if (!value && s != null)
                {
                    s.Activated = false;
                    if (s.Alive)
                    {
                        EntityManager.State.Remove(EntityID);
                    }
                }
                else if (value)
                {
                    if (s != null)
                    {
                        s.Activated = true;
                    }
                    else
                    {
                        EntityManager.State.Add(EntityID, new() { Activated = true });
                    }
                }
            }
        }

        public EntityPreset(Type creation_type, Vector2 position, Guid entityID, int frame, Permanence permanence = Permanence.GRID_LOCAL, string type = "")
        {
            Type = creation_type;
            Position = position;
            EntityID = entityID;
            Frame = frame;
            Permanence = permanence;
            TypeValue = type;
        }

        public Entity Create(Player p)
        {
            return (Entity)Activator.CreateInstance(Type, this, p);
        }

        public override string ToString()
        {
            return $"{Type.GetCustomAttribute<NamedEntity>().GetName(Type)} ({EntityID})";
        }
    }
}
