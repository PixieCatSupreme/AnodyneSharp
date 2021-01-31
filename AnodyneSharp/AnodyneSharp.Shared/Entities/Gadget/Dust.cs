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
        public bool IS_RAFT = false;

        public Dust(EntityPreset preset, Player p) : this(p)
        {
            Position = preset.Position;
            exists = true;
        }

        public Dust(Player p) : base(Vector2.Zero,"dust",16,16,Drawing.DrawOrder.BG_ENTITIES)
        {
            AddAnimation("poof", CreateAnimFrameArray(0, 1, 2, 3, 4), 13, false);
            AddAnimation("unpoof", CreateAnimFrameArray(3, 2, 1, 0), 13, false);
            SetFrame(0);
            b = p.broom;
            exists = false;
        }

        public override void Collided(Entity other)
        {
            if(!_curAnim.Finished && _curAnim.name == "unpoof" && b.dust == this)
            {
                exists = false;
                b.just_released_dust = false;
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            velocity = Vector2.Zero;
            ON_CONVEYOR = false;
            if(_curAnim.Finished && _curAnim.name == "poof")
            {
                exists = false;
            }
        }

        public override void Fall(Vector2 fallPoint)
        {
            if (IS_RAFT) return;

            if(_curAnim.name == "unpoof")
            {
                if(_curAnim.Finished)
                {
                    Play("poof");
                }
                return;
            }
        }

        public override void Conveyor(Touching direction)
        {
            if (direction != Touching.ANY && !IS_RAFT)
            {
                velocity = FacingDirection(FacingFromTouching(direction)) * 10;
            }
            ON_CONVEYOR = true;
        }
    }
}
