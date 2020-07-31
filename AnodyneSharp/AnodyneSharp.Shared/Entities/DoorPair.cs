using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    public class DoorMapPair
    {
        public EntityPreset Door { get; private set; }
        public string Map { get; private set; }

        public DoorMapPair(EntityPreset door, string map)
        {
            Door = door;
            Map = map;
        }
    }

    public class DoorPair
    {
        public DoorMapPair DoorOne { get; private set; }
        public DoorMapPair DoorTwo { get; private set; }

        public DoorPair (DoorMapPair doorOne, DoorMapPair doorTwo)
        {
            DoorOne = doorOne;
            DoorTwo = doorTwo;
        }

        public DoorMapPair GetLinkedDoor(EntityPreset door)
        {
            if (DoorOne.Door.EntityID == door.EntityID)
            {
                return DoorTwo;
            }
            else if (DoorTwo.Door.EntityID == door.EntityID)
            {
                return DoorOne;
            }
            else
            {
                throw new Exception($"No matching door found for door with id {door.EntityID}");
            }
        }
    }
}
