using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.GameEvents;
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

        public Facing facing;

        public ISpriteRenderer sprite;
        
        public string CurAnimName => sprite.CurAnimName;
        public bool AnimFinished => sprite.AnimFinished;
        public int FrameIndex => sprite.FrameIndex;
        public int Frame => sprite.Frame;

        private ILayerType layer_cache; //TODO: remove cache when layer setting is fully moved to sprite init
        public DrawOrder layer { set { layer_def = new Layer(value, this); } }
        public ILayerType layer_def { get { return sprite?.Layer ?? layer_cache; } set { if (sprite is null) layer_cache = value; else sprite.Layer = value; } }

        public Vector2 offset = Vector2.Zero;
        public float opacity = 1f;
        public float scale = 1f;
        protected SpriteEffects _flip = SpriteEffects.None;
        public float y_push = 0f; //sinking into the ground

        public bool _flickering { get; protected set; }
        private float _flickerTimer;
        private float _flickerFreq = 0;

        public Shadow shadow;

        public bool MapInteraction = true;
        public bool HoleAsWall = false;

        public Vector2 VisualCenter
        {
            get
            {
                return Position - offset + new Vector2(sprite.Width / 2, sprite.Height / 2);
            }
        }

        public Entity(Vector2 pos, int w=0, int h=0) : base(pos,w,h)
        {
            visible = false;
        }

        public Entity(Vector2 pos, DrawOrder layer)
            : base(pos)
        {
            this.layer = layer;
        }

        public Entity(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, frameWidth, frameHeight)
        {
            sprite = new StaticSpriteRenderer(textureName, frameWidth, frameHeight, 0, new Layer(layer, this));
        }

        public Entity(Vector2 pos, string textureName, int frameWidth, int frameHeight, ILayerType layer) 
            : base(pos,frameWidth, frameHeight)
        {
            sprite = new StaticSpriteRenderer(textureName,frameWidth, frameHeight, 0, layer);
        }

        public Entity(Vector2 pos, ISpriteRenderer sprite) : base(pos,sprite.Width,sprite.Height)
        {
            this.sprite = sprite;
        }

        public Entity(Vector2 pos, ISpriteRenderer sprite, DrawOrder layer) : base(pos,sprite.Width,sprite.Height)
        {
            this.sprite = sprite;
            sprite.Layer = new Layer(layer, this);
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
        public void Play(string AnimName, bool Force = false, int? newFramerate = null)
        {
            if (sprite.PlayAnim(AnimName, Force, newFramerate))
            {
                AnimationChanged(AnimName);
            }
        }

        public virtual void Draw()
        {
            DrawImpl();
        }

        protected void DrawImpl()
        {
            if (exists)
            {
                if (visible)
                {
                    sprite.Draw(SpriteDrawer._spriteBatch, Position - offset * scale, scale, (int)y_push, rotation, opacity, _flip);
                }
                shadow?.Draw();
                if (GlobalState.draw_hitboxes && HasVisibleHitbox)
                {
                    SpriteDrawer.DrawSprite(ResourceManager.GetTexture("hitbox"), Hitbox, color: Color.Red, Z: DrawingUtilities.GetDrawingZ(DrawOrder.HITBOX, 0));
                }
            }
        }

        public void SetFrame(int frame)
        {
            sprite.SetFrame(frame);
        }

        protected virtual bool SetTexture(string textureName, int frameWidth, int frameHeight, bool ignoreChaos = false, bool allowFailure = false)
        {
            if (sprite is null)
            {
                sprite = new StaticSpriteRenderer(textureName, frameWidth, frameHeight, 0, layer_cache);
                return true;
            }
            return sprite.SetTexture(textureName, frameWidth, frameHeight, ignoreChaos, allowFailure);
        }

        public virtual void ReloadTexture(bool ignoreChaos = false)
        {
            sprite.ReloadTexture(ignoreChaos);
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

        public static Facing FlipFacing(Facing f)
        {
            return f switch
            {
                Facing.LEFT => Facing.RIGHT,
                Facing.RIGHT => Facing.LEFT,
                Facing.UP => Facing.DOWN,
                _ => Facing.UP,
            };
        }

        public void FaceTowards(Vector2 target)
        {
            Vector2 dir = target - Position;
            if (MathF.Abs(dir.X) > MathF.Abs(dir.Y))
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

        protected virtual void CenterOffset(bool updatePos = true)
        {
            offset = (new Vector2(sprite?.Width ?? 16, sprite?.Height ?? 16) - new Vector2(width, height)) / 2;
            if (updatePos)
                Position += offset;
        }

        public void Flicker(float duration)
        {
            if (duration == 0f)
            {
                _flickering = false;
                visible = true;
                return;
            }
            _flickering = true;
            _flickerTimer = duration;
        }

        private void DoFlicker()
        {
            _flickerFreq -= GameTimes.DeltaTime;
            if (_flickerFreq <= 0)
            {
                _flickerFreq = FlickerLength;
                visible = !visible;
            }

            if (_flickerTimer > 0)
            {
                _flickerTimer -= GameTimes.DeltaTime;

                if (_flickerTimer <= 0)
                {
                    _flickering = false;
                    visible = true;
                }
            }
        }

        public override void Update()
        {
            base.Update();


            shadow?.Update();

            if (_flickering)
            {
                DoFlicker();
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            sprite?.Update();

            shadow?.PostUpdate();

        }

        public virtual void Collided(Entity other) { }

        /**
         * Sub entities that require collision and updates
         */
        public virtual IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>();
        }

        //Map interactions

        public virtual void Fall(Vector2 fallPoint)
        {
            exists = false;
        }



        public virtual void SlowTile() { }

        public virtual void Puddle() { }

        public virtual void Reflection() { }

        public virtual void Ladder() { }

        public virtual void Conveyor(Touching direction) { }

        public virtual void Grass() { }

        public virtual void OnEvent(GameEvent e) { }
    }
}
