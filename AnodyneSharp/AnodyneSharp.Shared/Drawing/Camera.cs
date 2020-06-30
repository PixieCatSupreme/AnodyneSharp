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
        public Vector3 Position { get; private set; }
        public Vector3 Offset { get; set; }
        public float Zoom { get; set; }

        private float _screenWidth;
        private float _screenHeight;

        public Vector2 Position2D
        {
            get
            {
                return new Vector2(Position.X,Position.Y) - new Vector2(SCREEN_WIDTH_IN_PIXELS, SCREEN_HEIGHT_IN_PIXELS) / 2;
            }
        }

        public Camera()
        {
            Position = Vector3.Zero;
            Zoom = 1;
        }

        public Camera(Vector2 position)
        {
            Position = new Vector3(position, 0);
            Zoom = 1;
        }

        public Camera(float x, float y)
        {
            Position = new Vector3(x, y, 0);
            Zoom = 1;
        }

        /// <summary>
        /// Initializes the camera and sets the view.
        /// </summary>
        /// <param name="screenWidth"></param>
        /// <param name="screenHeight"></param>
        public void Initialize(float screenWidth, float screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        /// <summary>
        /// Resets the camera to default zoom, offset and position.
        /// </summary>
        public void Reset()
        {
            Offset = Vector3.Zero;
            Zoom = 1f;
            Position = Vector3.Zero;
        }

        /// <summary>
        /// Updates the camera to simply looks in front of itself.
        /// </summary>
        public void Update()
        {
            View = Matrix.CreateLookAt(Position, new Vector3(Position.X, Position.Y, -1), Vector3.Up);

            Transform = Matrix.CreateTranslation(-Position) *
                                         //Matrix.CreateRotationZ(Rotation) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(160 * 0.5f, 160 * 0.5f, 0)) *
                                         Matrix.CreateTranslation(Offset);
        }

        public void Move(float x, float y)
        {
            Position += new Vector3(x, y, 0);
        }

        public void GoTo(float x, float y)
        {
            GoTo(new Vector2(x, y));
        }

        public void GoTo(Vector2 target)
        {
            target += new Vector2(SCREEN_WIDTH_IN_PIXELS, SCREEN_HEIGHT_IN_PIXELS) / 2;
            Position = new Vector3(target, Position.Z);
            Update();
        }
    }
}
