﻿using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
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
        public bool HasVisibleHitbox = false;

        private const float FlickerLength = 0.05f;

        public Spritesheet sprite;

        public Vector2 offset;
        public Facing facing;
        public DrawOrder layer;

        public Color color;

        protected Anim _curAnim;
        public string CurAnimName => _curAnim.name;
        public float opacity;

        public Shadow shadow;

        public float scale;

        protected SpriteEffects _flip;

        private List<Anim> _animations;
        private string textureName;

        protected bool _flickering;
        private float _flickerTimer;
        private float _flickerFreq = 0;

        private float _lastScale;

        public bool MapInteraction = true;

        public float y_push = 0f; //sinking into the ground

        public Vector2 VisualCenter
        {
            get
            {
                return Position - offset + new Vector2(sprite.Width / 2, sprite.Height / 2);
            }
        }

        public Entity(Vector2 pos, DrawOrder layer)
            : base(pos)
        {
            _animations = new List<Anim>();

            this.layer = layer;
            opacity = 1f;

            scale = 1;
            _lastScale = scale;

            color = Color.White;

            SetFrame(0);
        }

        public Entity(Vector2 pos, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, frameWidth, frameHeight)
        {
            _animations = new List<Anim>();

            this.layer = layer;
            opacity = 1f;

            scale = 1;
            _lastScale = scale;

            color = Color.White;

            SetFrame(0);
        }

        public Entity(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, frameWidth, frameHeight)
        {
            _animations = new List<Anim>();

            this.layer = layer;

            opacity = 1f;

            scale = 1;
            _lastScale = scale;

            color = Color.White;

            SetTexture(textureName, frameWidth, frameHeight);
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
            if (!Force && _curAnim != null && AnimName == _curAnim.name && !_curAnim.Finished)
            {
                return;
            }

            for (int i = 0; i < _animations.Count; i++)
            {
                if (_animations[i].name == AnimName)
                {
                    _curAnim = _animations[i];
                    _curAnim.Reset();

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
                Rectangle srect = sprite.GetRect(_curAnim.Frame);
                srect.Height -= (int)y_push;
                SpriteDrawer.DrawSprite(sprite.Tex,
                    MathUtilities.CreateRectangle(Position.X - offset.X * scale, Position.Y - offset.Y * scale + (int)y_push, srect.Width * scale, srect.Height * scale),
                    srect,
                    color * opacity,
                    rotation,
                    _flip,
                    DrawingUtilities.GetDrawingZ(layer, MapUtilities.GetInGridPosition(Position).Y+height));

                if (shadow != null)
                {
                    shadow.Draw();
                }
            }
            if(GlobalState.draw_hitboxes && HasVisibleHitbox && exists)
            {
                SpriteDrawer.DrawSprite(ResourceManager.GetTexture("frame_icon"), Hitbox, color:Color.Red, Z: DrawingUtilities.GetDrawingZ(DrawOrder.HITBOX));
            }
        }

        public void SetFrame(int frame)
        {
            _curAnim = new Anim("forcedFrame", new int[] { frame }, 1);
        }

        protected void UpdateAnimation()
        {
            if (_curAnim != null)
            {
                _curAnim.Update();

                _curAnim.Dirty = false;
            }

        }

        protected int[] CreateAnimFrameArray(params int[] frames)
        {
            return frames;
        }

        protected virtual bool SetTexture(string textureName, int frameWidth, int frameHeight, bool ignoreChaos = false, bool allowFailure = false)
        {
            this.textureName = textureName;
            sprite = new Spritesheet(ResourceManager.GetTexture(textureName, ignoreChaos,allowFailure), frameWidth, frameHeight);

            SetFrame(0);

            return sprite.Tex != null;
        }

        public virtual void ReloadTexture(bool ignoreChaos = false)
        {
            sprite = new Spritesheet(ResourceManager.GetTexture(textureName, ignoreChaos), sprite.Width, sprite.Height);
        }

        public static Facing FacingFromTouching(Touching t)
        {
            return t switch
            {
                Touching.NONE => throw new InvalidCastException(),
                Touching.LEFT => Facing.LEFT,
                Touching.RIGHT => Facing.RIGHT,
                Touching.UP => Facing.UP,
                Touching.DOWN => Facing.DOWN,
                Touching.ANY => throw new InvalidCastException(),
                _ => throw new InvalidCastException()
            };
        }

        public void FaceTowards(Vector2 target)
        {
            Vector2 dir = target - Position;
            if(MathF.Abs(dir.X) > MathF.Abs(dir.Y))
            {
                facing = dir.X > 0 ? Facing.RIGHT : Facing.LEFT;
            }
            else
            {
                facing = dir.Y > 0 ? Facing.DOWN : Facing.UP;
            }
        }

        public static Vector2 FacingDirection(Facing f)
        {
            return new Vector2(
                f == Facing.RIGHT ? 1 : (f == Facing.LEFT ? -1 : 0),
                f == Facing.DOWN ? 1 : (f == Facing.UP ? -1 : 0));
        }

        protected virtual void CenterOffset()
        {
            offset = (new Vector2(sprite.Width, sprite.Height) - new Vector2(width, height)) / 2;
            Position += offset;
        }

        public void Flicker(float duration)
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

        //Map interactions

        public virtual void Fall(Vector2 fallPoint)
        {
            exists = false;
        }

        public virtual void SlowTile() { }

        public virtual void Puddle() { }

        public virtual void Ladder() { }

        public virtual void Conveyor(Touching direction) { }

        public virtual void Grass() { }

        public virtual void OnEvent(GameEvent e) { }
    }
}
