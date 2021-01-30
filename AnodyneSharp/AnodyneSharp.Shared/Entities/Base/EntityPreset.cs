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

    public class EntityPreset
    {

        public Vector2 GridPosition
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
        public Permanence Permanence {get; private set;}
        public string TypeValue { get; private set; }
        public bool Alive { get; set; } = true;
        public bool Activated = false; //State-keeping for entities that need it beyond getting killed

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
