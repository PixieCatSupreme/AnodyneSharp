using AnodyneSharp.Logging;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;

namespace AnodyneSharp.Entities
{
    [Flags]
    public enum Touching
    {
        NONE = 0,
        LEFT = 1,
        RIGHT = 2,
        UP = 4,
        DOWN = 8,
        ANY = LEFT | RIGHT | UP | DOWN
    }

    public class GameObject
    {
        public const float OVERLAP_BIAS = 4;

        public Rectangle Hitbox
        {
            get
            {
                return MathUtilities.CreateRectangle(Position.X, Position.Y, width, height);
            }
        }

        public Vector2 Center
        {
            get
            {
                return Position + new Vector2(width, height) / 2;
            }
        }

        public bool Solid
        {
            get { return allowCollisions != Touching.NONE; }
            set
            {
                allowCollisions = value ? Touching.ANY : Touching.NONE;
            }
        }

        public Vector2 Position;
        public Vector2 lastPosition;
        public Vector2 velocity;
        public Vector2 acceleration;
        public Vector2 drag;

        public bool immovable;

        public float rotation;
        public float angularVelocity;
        public float angularAcceleration;

        //exists affects whether this object gets updates/collision/drawn, visible only affects drawing
        public bool exists = true;
        public bool visible;

        public int width;
        public int height;
        public Touching touching = Touching.NONE;
        public Touching wasTouching = Touching.NONE;
        public Touching allowCollisions = Touching.ANY;

        public GameObject(Vector2 pos)
        {
            Position = pos;
            lastPosition = pos;

            visible = true;
        }

        public GameObject(Vector2 pos, int width, int height)
            :this(pos)
        {
            this.width = width;
            this.height = height;
        }

        public GameObject(int width, int height)
        {
            this.width = width;
            this.height = height;

            visible = true;
        }

        public virtual void Update()
        {

        }

        public virtual void PostUpdate()
        {
            lastPosition = Position;
            Position += velocity * GameTimes.DeltaTime;

            velocity += acceleration * GameTimes.DeltaTime;
            velocity.X = CalculateDrag(velocity.X, drag.X);
            velocity.Y = CalculateDrag(velocity.Y, drag.Y);

            rotation += angularVelocity * GameTimes.DeltaTime;

            angularVelocity += angularAcceleration *GameTimes.DeltaTime;


            wasTouching = touching;
            touching = Touching.NONE;
        }

        /**
         * Slightly changed the original's separation logic below to accomodate for this game's specific usage.
         * 
         * 1. No objects in the game go fast enough for the check for overlapLeft/Down < overlapRight/Up to matter in comparison to using the position delta's to determine left/right or down/up
         *    This is in principle to check for objects completely passing each other in a single update
         * 2. Entity-Entity vs Entity-Tile collision gives Tiles priority by allowing them to overlap for more pixels before giving up on doing separation.
         *    Thereby preventing Entity-Entity collisions pushing one out of the map.
         */
        public static bool Separate(GameObject Object1, GameObject Object2)
        {
            bool separatedX = SeparateX(Object1, Object2);
            bool separatedY = SeparateY(Object1, Object2);
            return separatedX || separatedY;
        }

        /**
        * The X-axis component of the object separation process.
        * 
        * @param	Object1 	Any <code>FlxObject</code>.
        * @param	Object2		Any other <code>FlxObject</code>.
        * 
        * @return	Whether the objects in fact touched and were separated along the X axis.
        */
        public static bool SeparateX(GameObject Object1, GameObject Object2)
        {
            //can't separate two immovable objects
            bool obj1immovable = Object1.immovable;
            bool obj2immovable = Object2.immovable;
            if (obj1immovable && obj2immovable)
                return false;

            //First, get the two object deltas
            float overlap = 0;
            float obj1delta = Object1.Position.X - Object1.lastPosition.X;
            float obj2delta = Object2.Position.X - Object2.lastPosition.X;

            //Check if the X hulls actually overlap
            float obj1deltaAbs = Math.Abs(obj1delta);
            float obj2deltaAbs = Math.Abs(obj2delta);
            Vector2 pos1 = new Vector2(Object1.Position.X - ((obj1delta > 0) ? obj1delta : 0), Object1.lastPosition.Y);
            Vector2 size1 = new Vector2(Object1.width + obj1deltaAbs, Object1.height);
            Vector2 pos2 = new Vector2(Object2.Position.X - ((obj2delta > 0) ? obj2delta : 0), Object2.lastPosition.Y);
            Vector2 size2 = new Vector2(Object2.width + obj2deltaAbs, Object2.height);
            if ((pos1.X + size1.X > pos2.X) && (pos1.X < pos2.X + size2.X) && (pos1.Y + size1.Y > pos2.Y) && (pos1.Y < pos2.Y + size2.Y))
            {
                float maxOverlap = obj1deltaAbs + obj2deltaAbs;
                //map tiles have priority
                if(Object2 is Entity)
                {
                    maxOverlap += OVERLAP_BIAS / 2;
                }
                else
                {
                    maxOverlap += OVERLAP_BIAS;
                }

                //If they did overlap (and can), figure out by how much and flip the corresponding flags
                float overlapRight = Object1.Position.X + Object1.width - Object2.Position.X;
                float overlapLeft = Object1.Position.X - Object2.width - Object2.Position.X;

                if (MathF.Abs(overlapRight) < MathF.Abs(overlapLeft))
                {
                    overlap = overlapRight;
                    if ((overlap > maxOverlap) || (Object1.allowCollisions & Touching.RIGHT) == 0 || (Object2.allowCollisions & Touching.LEFT) == 0)
                        overlap = 0;
                    else
                    {
                        Object1.touching |= Touching.RIGHT;
                        Object2.touching |= Touching.LEFT;
                    }
                }
                else
                {
                    overlap = overlapLeft;
                    if ((-overlap > maxOverlap) || (Object1.allowCollisions & Touching.LEFT) == 0 || (Object2.allowCollisions & Touching.RIGHT) == 0)
                        overlap = 0;
                    else
                    {
                        Object1.touching |= Touching.LEFT;
                        Object2.touching |= Touching.RIGHT;
                    }
                }
            }

            //Then adjust their positions and velocities accordingly (if there was any overlap)
            if (overlap != 0)
            {

                if (!obj1immovable && !obj2immovable)
                {
                    overlap *= 0.5f;
                    Object1.Position.X -= overlap;
                    Object2.Position.X += overlap;
                }
                else if (!obj1immovable)
                {
                    Object1.Position.X -= overlap;
                    Object1.lastPosition.X = Object1.Position.X;
                }
                else if (!obj2immovable)
                {
                    Object2.Position.X += overlap;
                    Object2.lastPosition.X = Object2.Position.X;
                }
                return true;
            }
            else
                return false;
        }

        /**
        * The Y-axis component of the object separation process.
        * 
        * @param	Object1 	Any <code>FlxObject</code>.
        * @param	Object2		Any other <code>FlxObject</code>.
        * 
        * @return	Whether the objects in fact touched and were separated along the Y axis.
        */
        public static bool SeparateY(GameObject Object1, GameObject Object2)
        {
            //can't separate two immovable objects
            bool obj1immovable = Object1.immovable;
            bool obj2immovable = Object2.immovable;
            if (obj1immovable && obj2immovable)
                return false;

            //First, get the two object deltas
            float overlap = 0;
            float obj1delta = Object1.Position.Y - Object1.lastPosition.Y;
            float obj2delta = Object2.Position.Y - Object2.lastPosition.Y;

            //Check if the Y hulls actually overlap
            float obj1deltaAbs = Math.Abs(obj1delta);
            float obj2deltaAbs = Math.Abs(obj2delta);
            Vector2 pos1 = new Vector2(Object1.lastPosition.X, Object1.Position.Y -((obj1delta > 0) ? obj1delta : 0));
            Vector2 size1 = new Vector2(Object1.width, Object1.height + obj1deltaAbs);
            Vector2 pos2 = new Vector2(Object2.lastPosition.X, Object2.Position.Y - ((obj2delta > 0) ? obj2delta : 0));
            Vector2 size2 = new Vector2(Object2.width, Object2.height + obj2deltaAbs);
            if ((pos1.X + size1.X > pos2.X) && (pos1.X < pos2.X + size2.X) && (pos1.Y + size1.Y > pos2.Y) && (pos1.Y < pos2.Y + size2.Y))
            {
                float maxOverlap = obj1deltaAbs + obj2deltaAbs;
                //map tiles have priority
                if (Object2 is Entity)
                {
                    maxOverlap += OVERLAP_BIAS / 2;
                }
                else
                {
                    maxOverlap += OVERLAP_BIAS;
                }

                //If they did overlap (and can), figure out by how much and flip the corresponding flags
                float overlapDown = Object1.Position.Y + Object1.height - Object2.Position.Y;
                float overlapUp = Object1.Position.Y - Object2.height - Object2.Position.Y;

                if (MathF.Abs(overlapDown) < MathF.Abs(overlapUp))
                {
                    overlap = overlapDown;
                    if ((overlap > maxOverlap) || (Object1.allowCollisions & Touching.DOWN) == 0 || (Object2.allowCollisions & Touching.UP) == 0)
                        overlap = 0;
                    else
                    {
                        Object1.touching |= Touching.DOWN;
                        Object2.touching |= Touching.UP;
                    }
                }
                else
                {
                    overlap = overlapUp;
                    if ((-overlap > maxOverlap) || (Object1.allowCollisions & Touching.UP) == 0 || (Object2.allowCollisions & Touching.DOWN) == 0)
                        overlap = 0;
                    else
                    {
                        Object1.touching |= Touching.UP;
                        Object2.touching |= Touching.DOWN;
                    }
                }
            }

            //Then adjust their positions and velocities accordingly (if there was any overlap)
            if (overlap != 0)
            {
                if (!obj1immovable && !obj2immovable)
                {
                    overlap *= 0.5f;
                    Object1.Position.Y -= overlap;
                    Object2.Position.Y += overlap;
                }
                else if (!obj1immovable)
                {
                    Object1.Position.Y -= overlap;
                    Object1.lastPosition.Y = Object1.Position.Y;
                }
                else if (!obj2immovable)
                {
                    Object2.Position.Y += overlap;
                    Object2.lastPosition.Y = Object2.Position.Y;
                }
                return true;
            }
            else
                return false;
        }

        protected void MoveTowards(Vector2 target, float speed)
        {
            velocity = target - Position;
            velocity.Normalize();
            velocity *= speed;
        }

        private float CalculateDrag(float velocity, float drag)
        {
            drag = drag * GameTimes.DeltaTime;
            if(velocity < 0)
            {
                velocity = Math.Min(velocity + drag, 0f);
            } else
            {
                velocity = Math.Max(velocity - drag, 0f);
            }
            return velocity;
        }
    }
}
