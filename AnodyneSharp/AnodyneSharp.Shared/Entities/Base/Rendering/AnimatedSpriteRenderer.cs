using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Logging;
using AnodyneSharp.Resources;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Entities.Base.Rendering
{
    public class AnimatedSpriteRenderer : ISpriteRenderer
    {
        string textureName;
        Spritesheet sprite;
        Dictionary<string,Anim> animations;
        Anim _curAnim;

        public Color Color { get; set; } = Color.White;

        public int Width => sprite.Width;
        public int Height => sprite.Height;

        public ILayerType Layer { get; set; }

        public string CurAnimName => _curAnim.name;
        public bool AnimFinished => _curAnim.Finished;
        public int FrameIndex => _curAnim.CurIndex;
        public int Frame => _curAnim.Frame;

        public AnimatedSpriteRenderer(string textureName, int frameWidth, int frameHeight, ILayerType layer, params Anim[] animations)
        {
            this.textureName = textureName;
            sprite = new(ResourceManager.GetTexture(textureName),frameWidth,frameHeight);
            this.animations = animations.ToDictionary(a => a.name);
            _curAnim = animations[0];
            this.Layer = layer;
        }
        public AnimatedSpriteRenderer(string textureName, int frameWidth, int frameHeight, params Anim[] animations)
        {
            this.textureName = textureName;
            sprite = new(ResourceManager.GetTexture(textureName), frameWidth, frameHeight);
            this.animations = animations.ToDictionary(a => a.name);
            _curAnim = animations[0];
        }

        public bool PlayAnim(string name, bool force = false, int? newFramerate = null)
        {
            if(!animations.TryGetValue(name,out Anim anim))
            {
                DebugLogger.AddWarning($"No animation called {name}");
                return false;
            }
            else if(!force && _curAnim == anim && !_curAnim.Finished)
            {
                return false;
            }
            _curAnim = anim;
            _curAnim.Reset();
            _curAnim.FrameRate = newFramerate ?? _curAnim.FrameRate;
            return true;
        }

        public void Update()
        {
            _curAnim.Update();
        }

        public bool SetTexture(string textureName, int width, int height, bool ignoreChaos, bool allowFailure)
        {
            this.textureName = textureName;
            sprite = new Spritesheet(ResourceManager.GetTexture(textureName, ignoreChaos, allowFailure), width, height);
            return sprite.Tex != null;
        }

        public void ReloadTexture(bool ignoreChaos = false)
        {
            sprite = new(ResourceManager.GetTexture(textureName,ignoreChaos), sprite.Width, sprite.Height);
        }

        public void Draw(Vector2 position, float scale, int y_push, float rotation, float opacity, SpriteEffects flip)
        {
            Rectangle srect = sprite.GetRect(_curAnim.Frame);
            srect.Height -= y_push;

            SpriteDrawer.DrawSprite(sprite.Tex,
                MathUtilities.CreateRectangle(position.X, position.Y + y_push, srect.Width * scale, srect.Height * scale),
                srect,
                Color * opacity,
                rotation,
                flip,
                Layer.Z);
        }

        public void SetFrame(int index)
        {
            throw new System.NotImplementedException();
        }
    }
}
