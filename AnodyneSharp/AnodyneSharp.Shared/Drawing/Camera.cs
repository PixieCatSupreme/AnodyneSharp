using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;

using static AnodyneSharp.Registry.GameConstants;

namespace AnodyneSharp.Drawing
{
    /// <summary>
    /// A camera objects which updates the view matrix and projection matrix.
    /// </summary>
    public class Camera
    {
        public Matrix View { get; private set; }
        public Matrix Transform { get; private set; }
        private Vector3 actual_pos;
        public Vector3 Position
        {
            get
            {
                return new Vector3((int)actual_pos.X, (int)actual_pos.Y, actual_pos.Z);
            }
        }
        public Vector3 Offset { get; set; }
        public float Zoom { get; set; }

        public Vector2 Position2D
        {
            get
            {
                return new Vector2(Position.X, Position.Y) - new Vector2(SCREEN_WIDTH_IN_PIXELS, SCREEN_HEIGHT_IN_PIXELS) / 2;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                Vector2 pos = Position2D;
                return new Rectangle((int)pos.X, (int)pos.Y, SCREEN_HEIGHT_IN_PIXELS, SCREEN_HEIGHT_IN_PIXELS);
            }
        }

        public Camera()
        {
            actual_pos = Vector3.Zero;
            Zoom = 1;
        }

        public Camera(Vector2 position)
        {
            actual_pos = new Vector3(position, 0);
            Zoom = 1;
        }

        public Camera(float x, float y)
        {
            actual_pos = new Vector3(x, y, 0);
            Zoom = 1;
        }

        /// <summary>
        /// Resets the camera to default zoom, offset and position.
        /// </summary>
        public void Reset()
        {
            Offset = Vector3.Zero;
            Zoom = 1f;
            actual_pos = Vector3.Zero;
        }

        /// <summary>
        /// Updates the camera to simply looks in front of itself.
        /// </summary>
        public void Recalc()
        {
            View = Matrix.CreateLookAt(Position, new Vector3(Position.X, Position.Y, -1), Vector3.Up);

            Transform = Matrix.CreateTranslation(-Position) *
                                         //Matrix.CreateRotationZ(Rotation) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(160 * 0.5f, 160 * 0.5f, 0)) *
                                         Matrix.CreateTranslation(Offset);
        }

        public bool GoTowards(Vector2 target, float speed)
        {
            return MathUtilities.MoveTo(ref actual_pos.X, target.X + SCREEN_WIDTH_IN_PIXELS / 2, speed) && MathUtilities.MoveTo(ref actual_pos.Y, target.Y + SCREEN_HEIGHT_IN_PIXELS / 2, speed);
        }

        public void GoTo(float x, float y)
        {
            GoTo(new Vector2(x, y));
        }

        public void GoTo(Vector2 target)
        {
            target += new Vector2(SCREEN_WIDTH_IN_PIXELS, SCREEN_HEIGHT_IN_PIXELS) / 2;
            actual_pos = new Vector3(target, actual_pos.Z);
            Recalc();
        }
    }
}
