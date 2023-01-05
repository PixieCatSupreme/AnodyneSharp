using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget
{
    class Explosion : Entity
    {
        private static AnimatedSpriteRenderer GetSprite()
        {
            int o = GlobalState.IsCell ? 5 : 0;
            return new AnimatedSpriteRenderer("enemy_explode_2", 24, 24, 
                new Anim("explode", new int[] { o, o + 1, o + 2, o + 3, o + 4 }, GlobalState.IsCell ? 10 : 12, false)
            );
        }

        public Explosion(Entity parent) : base(parent.Position - Vector2.One*4, GetSprite(), DrawOrder.BG_ENTITIES)
        {
            Sounds.SoundManager.PlaySoundEffect("hit_wall");
            Play("explode");
            layer_def = new RefLayer(parent.layer_def, 1);
        }
    }
}
