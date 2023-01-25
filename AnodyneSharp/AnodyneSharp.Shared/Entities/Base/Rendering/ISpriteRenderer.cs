using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Entities.Base.Rendering
{
    public interface ISpriteRenderer
    {
        bool AnimFinished { get; }
        Color Color { get; set; }
        string CurAnimName { get; }
        int Frame { get; }
        int FrameIndex { get; }
        int Height { get; }
        int Width { get; }
        ILayerType Layer { get; set; }

        void Draw(SpriteBatch batch, Vector2 position, float scale, int y_push, float rotation, float opacity, SpriteEffects flip);
        void SetFrame(int index);
        bool PlayAnim(string name, bool force = false, int? newFramerate = null);
        void ReloadTexture(bool ignoreChaos = false);
        bool SetTexture(string textureName, int width, int height, bool ignoreChaos, bool allowFailure);
        void Update();
    }
}