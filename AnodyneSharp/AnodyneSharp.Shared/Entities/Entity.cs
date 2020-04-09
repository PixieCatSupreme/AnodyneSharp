using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Animations;
using AnodyneSharp.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace AnodyneSharp.Entities
{
    public enum Facing
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    public class Entity
    {
        public Vector2 Position;
        public Texture2D Texture;

        public bool solid;
        public bool visible;

        protected Vector2 offset;
        protected Vector2 velocity;
        protected Facing facing;

        protected int _curFrame;
        protected int _curIndex;
        protected Anim _curAnim;

        private List<Anim> _animations;

        private bool dirty;
        private float _frameTimer;

        private bool finished;

		public int width;
		public int height;

        public int frameWidth;
        public int frameHeight;

        private Rectangle spriteRect;

        public Entity(Vector2 pos, int frameWidth, int frameHeight)
        {
            Position = pos;

            _animations = new List<Anim>();

            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
        }
        /**
 * Adds a new animation to the sprite.
 * 
 * @param	Name		What this animation should be called (e.g. "run").
 * @param	Frames		An array of numbers indicating what frames to play in what order (e.g. 1, 2, 3).
 * @param	FrameRate	The speed in frames per second that the animation should play at (e.g. 40 fps).
 * @param	Looped		Whether or not the animation is looped or just plays once.
 */
        public void AddAnimation(string name, int[] frames, float frameRate = 0, bool looped = true)
        {
            _animations.Add(new Anim(name, frames, frameRate, looped));
        }

        /**
 * Plays an existing animation (e.g. "run").
 * If you call an animation that is already playing it will be ignored.
 * 
 * @param	AnimName	The string name of the animation you want to play.
 * @param	Force		Whether to force the animation to restart.
 */
        public void Play(string AnimName, bool Force = false)
        {
            if (!Force && (_curAnim != null) && (AnimName == _curAnim.name) && (_curAnim.looped || !finished))
            {
                return;
            }
            _curFrame = 0;
            _curIndex = 0;
            _frameTimer = 0;

            for (int i = 0; i < _animations.Count; i++)
            {
                if (_animations[i].name == AnimName)
                {
                    _curAnim = _animations[i];
                    if (_curAnim.delay <= 0)
                    {
                        finished = true;
                    }
                    else
                    {
                        finished = false;
                    }

                    _curIndex = _curAnim.frames[_curFrame];
                    dirty = true;
                    return;
                }
            }
            DebugLogger.AddWarning("No animation called \"" + AnimName + "\"");
        }

        public virtual void Update()
        {
            Position += velocity;

            UpdateAnimation();
        }

        public void Draw()
        {
            SpriteDrawer.DrawSprite(Texture, new Rectangle((int)Position.X, (int)Position.Y, frameWidth, frameHeight), spriteRect);
        }

        protected void UpdateAnimation()
        {
            if ((_curAnim != null) && (_curAnim.delay > 0) && (_curAnim.looped || !finished))
            {
                _frameTimer += GameTimes.DeltaTime;
                while (_frameTimer > _curAnim.delay)
                {
                    _frameTimer = _frameTimer - _curAnim.delay;
                    if (_curFrame == _curAnim.frames.Length - 1)
                    {
                        if (_curAnim.looped)
                            _curFrame = 0;
                        finished = true;
                    }
                    else
                        _curFrame++;
                    _curIndex = _curAnim.frames[_curFrame];
                    dirty = true;
                }
            }


            if (dirty)
            {
                UpdateRect();
            }
        }

        protected void UpdateRect()
        {
            int indexX = _curIndex * frameWidth;
            int indexY = 0;

            //Handle sprite sheets
            int texWidth = Texture.Bounds.Width;
            if (indexX >= texWidth)
            {
                indexY = indexX / texWidth * frameHeight;
                indexX %= texWidth;
            }

            spriteRect = new Rectangle(indexX, indexY, frameWidth, frameHeight);

            dirty = false;
        }

        protected int[] CreateAnimFrameArray(params int[] frames)
        {
            return frames;
        }
    }
}
