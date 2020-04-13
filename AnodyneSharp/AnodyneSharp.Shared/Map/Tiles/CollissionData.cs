using AnodyneSharp.Entities;

namespace AnodyneSharp.Map.Tiles
{
    public struct CollissionData
    {


        public int Start { get; private set; }
        public int? End { get; private set; }
        public Touching AllowedCollisions { get; private set; }
        public CollisionEventType CollisionEventType { get; private set; }

        public CollissionData(int start, Touching allowedCollisions, CollisionEventType collisionEventType = CollisionEventType.NONE)
        {
            Start = start;
            End = null;
            AllowedCollisions = allowedCollisions;
            CollisionEventType = collisionEventType;
        }

        public CollissionData(int start, int end, Touching allowedCollisions, CollisionEventType collisionEventType = CollisionEventType.NONE)
        {
            Start = start;
            End = end;
            AllowedCollisions = allowedCollisions;
            CollisionEventType = collisionEventType;
        }
    }
}
