using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

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
        }
    }
}
