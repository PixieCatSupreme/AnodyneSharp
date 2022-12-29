using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget
{
    class Explosion : Entity
    {
        public Explosion(Entity parent) : base(parent.Position - Vector2.One*4, "enemy_explode_2", 24,24,DrawOrder.BG_ENTITIES)
        {
            Sounds.SoundManager.PlaySoundEffect("hit_wall");
            int i = GlobalState.IsCell ? 5 : 0;
            AddAnimation("explode", CreateAnimFrameArray(i, i + 1, i + 2, i + 3, i + 4), GlobalState.IsCell ? 10 : 12, false);
            Play("explode");
            layer_def = new RefLayer(parent.layer_def, 1);
        }
    }
}
