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

        public bool solid
        {
            get { return allowCollisions != Touching.NONE; }
            set
            {
                if (value)
                {
                    allowCollisions = Touching.ANY;
                } else
                {
                    allowCollisions = Touching.NONE;
                }
            }
        }

        public Vector2 Position;
        public Vector2 lastPosition;
        public Vector2 velocity;
        public Vector2 acceleration;

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

            rotation += angularVelocity * GameTimes.DeltaTime;

            angularVelocity += angularAcceleration *GameTimes.DeltaTime;

            wasTouching = touching;
            touching = Touching.NONE;
        }

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
            if (obj1delta != obj2delta)
            {
                //Check if the X hulls actually overlap
                float obj1deltaAbs = Math.Abs(obj1delta);
                float obj2deltaAbs = Math.Abs(obj2delta);
                Vector2 pos1 = new Vector2(Object1.Position.X - ((obj1delta > 0) ? obj1delta : 0), Object1.lastPosition.Y);
                Vector2 size1 = new Vector2(Object1.width + obj1deltaAbs, Object1.height);
                Vector2 pos2 = new Vector2(Object2.Position.X - ((obj2delta > 0) ? obj2delta : 0), Object2.lastPosition.Y);
                Vector2 size2 = new Vector2(Object2.width + obj2deltaAbs, Object2.height);
                if ((pos1.X + size1.X > pos2.X) && (pos1.X < pos2.X + size2.X) && (pos1.Y + size1.Y > pos2.Y) && (pos1.Y < pos2.Y + size2.Y))
                {
                    float maxOverlap = obj1deltaAbs + obj2deltaAbs + OVERLAP_BIAS;

                    //If they did overlap (and can), figure out by how much and flip the corresponding flags
                    if (obj1delta > obj2delta)
                    {
                        overlap = Object1.Position.X + Object1.width - Object2.Position.X;
                        if ((overlap > maxOverlap) || (Object1.allowCollisions & Touching.RIGHT) == 0 || (Object2.allowCollisions & Touching.LEFT) == 0)
                            overlap = 0;
                        else
                        {
                            Object1.touching |= Touching.RIGHT;
                            Object2.touching |= Touching.LEFT;
                        }
                    }
                    else if (obj1delta < obj2delta)
                    {
                        overlap = Object1.Position.X - Object2.width - Object2.Position.X;
                        if ((-overlap > maxOverlap) || (Object1.allowCollisions & Touching.LEFT) == 0 || (Object2.allowCollisions & Touching.RIGHT) == 0)
                            overlap = 0;
                        else
                        {
                            Object1.touching |= Touching.LEFT;
                            Object2.touching |= Touching.RIGHT;
                        }
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
                }
                else if (!obj2immovable)
                {
                    Object2.Position.X += overlap;
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
            if (obj1delta != obj2delta)
            {
                //Check if the Y hulls actually overlap
                float obj1deltaAbs = Math.Abs(obj1delta);
                float obj2deltaAbs = Math.Abs(obj2delta);
                Vector2 pos1 = new Vector2(Object1.lastPosition.X, Object1.Position.Y -((obj1delta > 0) ? obj1delta : 0));
                Vector2 size1 = new Vector2(Object1.width, Object1.height + obj1deltaAbs);
                Vector2 pos2 = new Vector2(Object2.lastPosition.X, Object2.Position.Y - ((obj2delta > 0) ? obj2delta : 0));
                Vector2 size2 = new Vector2(Object2.width, Object2.height + obj2deltaAbs);
                if ((pos1.X + size1.X > pos2.X) && (pos1.X < pos2.X + size2.X) && (pos1.Y + size1.Y > pos2.Y) && (pos1.Y < pos2.Y + size2.Y))
                {
                    float maxOverlap = obj1deltaAbs + obj2deltaAbs + OVERLAP_BIAS;

                    //If they did overlap (and can), figure out by how much and flip the corresponding flags
                    if (obj1delta > obj2delta)
                    {
                        overlap = Object1.Position.Y + Object1.height - Object2.Position.Y;
                        if ((overlap > maxOverlap) || (Object1.allowCollisions & Touching.DOWN) == 0 || (Object2.allowCollisions & Touching.UP) == 0)
                            overlap = 0;
                        else
                        {
                            Object1.touching |= Touching.DOWN;
                            Object2.touching |= Touching.UP;
                        }
                    }
                    else if (obj1delta < obj2delta)
                    {
                        overlap = Object1.Position.Y - Object2.height - Object2.Position.Y;
                        if ((-overlap > maxOverlap) || (Object1.allowCollisions & Touching.UP) == 0 || (Object2.allowCollisions & Touching.DOWN) == 0)
                            overlap = 0;
                        else
                        {
                            Object1.touching |= Touching.UP;
                            Object2.touching |= Touching.DOWN;
                        }
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
                }
                else if (!obj2immovable)
                {
                    Object2.Position.Y += overlap;
                }
                return true;
            }
            else
                return false;
        }
    }
}
