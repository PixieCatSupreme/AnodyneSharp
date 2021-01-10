using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    [Collision(MapCollision = true)]
    public class Foot_Overlay : Entity
    {
        Player follow;
        bool activated = true;
        public Foot_Overlay(Player p) : base(p.Position, "overlay_water", 24, 24, Drawing.DrawOrder.FOOT_OVERLAY)
        {
            follow = p;
            visible = false;
            AddAnimation("water", CreateAnimFrameArray(0, 1), 5);
            AddAnimation("grass_go", CreateAnimFrameArray(4, 5), 8);
            AddAnimation("grass_stop", CreateAnimFrameArray(5), 8);
        }

        public void OnMapChange()
        {
            if (!SetTexture($"overlay_{GlobalState.CURRENT_MAP_NAME}_water", 24, 24,allowFailure:true))
            {
                SetTexture("overlay_water", 24, 24);
                offset.Y = 0;
                layer = Drawing.DrawOrder.FOOT_OVERLAY;
            }
            Play("water");
        }

        public override void Update()
        {
            base.Update();

            if (GlobalState.CURRENT_MAP_NAME == "WINDMILL") //only map that doesn't have the foot overlay flicker
            {
                visible = true;
            }
        }

        public override void Grass()
        {
            if (_curAnim.name == "water")
            {
                SetTexture($"overlay_{GlobalState.CURRENT_MAP_NAME}_grass", 24, 24);
                offset.Y = 1;
                layer = Drawing.DrawOrder.FG_SPRITES;
            }
            activated = true;
            visible = true;

            if (follow.velocity == Vector2.Zero)
            {
                Play($"grass_stop");
            }
            else
            {
                Play($"grass_go");
            }
        }

        public override void Conveyor(Touching direction)
        {
            Activate();
        }

        public override void Puddle()
        {
            Activate();
        }

        private void Activate()
        {
            if (_curAnim.name != "water")
            {
                OnMapChange();
            }
            if (follow.state == PlayerState.GROUND)
            {
                activated = true;
                if (GlobalState.CURRENT_MAP_NAME == "WINDMILL")
                {
                    visible = true;
                }
                else
                {
                    Flicker(0.1f);
                }
            }
        }

        public override void Fall(Vector2 fallPoint)
        {
            //Nothing
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            Position = follow.Position - new Vector2(7, 3);
            if (follow.facing == Facing.RIGHT)
            {
                Position.X--;
            }

            if (!activated)
            {
                visible = false;
                _flickering = false;
            }
            activated = false;
        }
    }
}
