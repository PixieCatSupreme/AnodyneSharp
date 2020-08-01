using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Animations;
using AnodyneSharp.Logging;
using AnodyneSharp.Resources;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

    public class Entity : GameObject
    {
        private const float FlickerLength = 0.05f;

        public Texture2D Texture;

        public int frameWidth;
        public int frameHeight;
        public Vector2 offset;
        public Facing facing;
        public DrawOrder layer;


        public Color color;

        public bool finished;

        protected int _curIndex;
        protected int _curFrame;
        protected Anim _curAnim;
        protected float _opacity;

        protected Rectangle spriteRect;

        protected Shadow shadow;

        protected float scale;

        protected SpriteEffects _flip;

        private List<Anim> _animations;
        private string textureName;

        private bool dirty;
        private float _frameTimer;

        protected bool _flickering;
        private float _flickerTimer;
        private float _flickerFreq = 0;

        private float _lastScale;

        public Vector2 VisualCenter
        {
            get
            {
                return Position - offset + new Vector2(frameWidth/2,frameHeight/2);
            }
        }

        public Entity(Vector2 pos, DrawOrder layer)
            : base(pos)
        {
            _animations = new List<Anim>();

            frameWidth = 0;
            frameHeight = 0;

            this.layer = layer;
            _opacity = 1f;

            scale = 1;
            _lastScale = scale;

            color = Color.White;
        }

        public Entity(Vector2 pos, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, frameWidth, frameHeight)
        {
            _animations = new List<Anim>();

            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;

            this.layer = layer;
            _opacity = 1f;

            scale = 1;
            _lastScale = scale;

            color = Color.White;
        }

        public Entity(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, frameWidth, frameHeight)
        {
            _animations = new List<Anim>();

            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;

            this.layer = layer;

            _opacity = 1f;

            scale = 1;
            _lastScale = scale;

            color = Color.White;

            SetTexture(textureName);
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

        protected virtual void AnimationChanged(string name) { }

        /**
         * Plays the animation corresponding to AnimName combined with '_x' where x is u,l,d, or r depending on facing.
         */
        public void PlayFacing(string AnimName)
        {
            char append = Enum.GetNames(typeof(Facing))[(int)facing].ToLower()[0];
            Play($"{AnimName}_{append}");
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
            _curIndex = 0;
            _curFrame = 0;
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

                    _curFrame = _curAnim.frames[_curIndex];
                    dirty = true;
                    AnimationChanged(AnimName);
                    return;
                }
            }
            DebugLogger.AddWarning("No animation called \"" + AnimName + "\"");
        } 

        public override void Update()
        {
            base.Update();

            if (shadow != null)
            {
                shadow.Update();
            }

            if (_flickering)
            {
                DoFlicker();
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            UpdateAnimation();

            if (shadow != null)
            {
                shadow.PostUpdate();
            }

        }

        public virtual void Collided(Entity other) { }

        /**
         * Sub entities that require collision and updates
         */
        public virtual IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>();
        }

        public virtual void Draw()
        {
            if (visible && exists)
            {
                SpriteDrawer.DrawSprite(Texture, 
                    MathUtilities.CreateRectangle(Position.X - offset.X*scale, Position.Y - offset.Y*scale, frameWidth*scale, frameHeight*scale), 
                    spriteRect,
                    color * _opacity,
                    rotation,
                    _flip,
                    DrawingUtilities.GetDrawingZ(layer, MapUtilities.GetInGridPosition(Position).Y));
                
                if (shadow != null)
                {
                    shadow.Draw();
                }
            }

        }

        public void SetFrame(int frame)
        {
            _curFrame = frame;

            UpdateRect();
        }

        protected void UpdateAnimation()
        {
            if ((_curAnim != null) && (_curAnim.delay > 0) && (_curAnim.looped || !finished))
            {
                _frameTimer += GameTimes.DeltaTime;
                while (_frameTimer > _curAnim.delay)
                {
                    _frameTimer -= _curAnim.delay;
                    if (_curIndex == _curAnim.frames.Length - 1)
                    {
                        if (_curAnim.looped)
                            _curIndex = 0;
                        finished = true;
                    }
                    else
                        _curIndex++;
                    _curFrame = _curAnim.frames[_curIndex];
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
            int indexX = _curFrame * frameWidth;
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

        protected virtual bool SetTexture(string textureName, bool ignoreChaos = false)
        {
            this.textureName = textureName;
            Texture = ResourceManager.GetTexture(textureName, ignoreChaos);

            return Texture != null;
        }

        public virtual void ReloadTexture(bool ignoreChaos = false)
        {
            Texture = ResourceManager.GetTexture(textureName, ignoreChaos);
        }

        public static Vector2 FacingDirection(Facing f)
        {
            return new Vector2(
                f == Facing.RIGHT ? 1 : (f == Facing.LEFT ? -1 : 0),
                f == Facing.DOWN ? 1 : (f == Facing.UP ? -1 : 0));
        }

        protected void Flicker(float duration)
        {
            _flickering = true;
            _flickerTimer = duration;
        }

        private void DoFlicker()
        {
            if (_flickerTimer > 0)
            {
                _flickerTimer -= GameTimes.DeltaTime;
                _flickerFreq -= GameTimes.DeltaTime;
                if (_flickerFreq <= 0)
                {
                    _flickerFreq = FlickerLength;
                    visible = !visible;
                }
            }
            else
            {
                _flickering = false;
                visible = true;
            }
        }
    }
}
