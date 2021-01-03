using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    [NamedEntity, Collision(typeof(Dust), MapCollision = true, KeepOnScreen = true)]
    public class Dust : Entity
    {
        private Broom b; //used to un-unpoof on dust-dust collision

        public bool ON_CONVEYOR = false;

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

        public override void PostUpdate()
        {
            base.PostUpdate();
            velocity = Vector2.Zero;
            ON_CONVEYOR = false;
        }

        public override void Fall(Vector2 fallPoint)
        {
            if(_curAnim.name == "unpoof")
            {
                if(_curAnim.Finished)
                {
                    Play("poof");
                }
                return;
            }

            if(_curAnim.Finished)
            {
                exists = false;
            }
        }

        public override void Conveyor(Touching direction)
        {
            if (direction != Touching.ANY)
            {
                velocity = FacingDirection(FacingFromTouching(direction)) * 10;
            }
            ON_CONVEYOR = true;
        }
    }
}
