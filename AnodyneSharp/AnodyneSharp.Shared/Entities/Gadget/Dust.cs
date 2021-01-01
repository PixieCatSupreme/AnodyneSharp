using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    [NamedEntity, Collision(typeof(Dust))]
    public class Dust : Entity
    {
        private Broom b; //used to un-unpoof on dust-dust collision

        public Dust(EntityPreset preset, Player p) : base(preset.Position,"dust",16,16,Drawing.DrawOrder.BG_ENTITIES)
        {
            AddAnimation("poof", CreateAnimFrameArray(0, 1, 2, 3, 4), 13, false);
            AddAnimation("unpoof", CreateAnimFrameArray(3, 2, 1, 0), 13, false);
            SetFrame(0);
            b = p.broom;
        }

        public override void Collided(Entity other)
        {
            if(!_curAnim.Finished)
            {
                exists = false;
                b.dust = this;
                b.just_released_dust = false;
            }
        }
    }
}
