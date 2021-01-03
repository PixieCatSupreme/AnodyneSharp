using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    public class Foot_Overlay : Entity
    {
        Player follow;
        public Foot_Overlay(Player p) : base(p.Position, "overlay_water", 24, 24, Drawing.DrawOrder.FOOT_OVERLAY)
        {
            follow = p;
            visible = false;
            AddAnimation("water", CreateAnimFrameArray(0, 1), 5);
        }

        public void OnMapChange()
        {
            if (!SetTexture($"overlay_{GlobalState.CURRENT_MAP_NAME}_water", 24, 24))
            {
                SetTexture("overlay_water", 24, 24);
            }
            Play("water");
        }

        public override void Update()
        {
            base.Update();
            if (!follow.ON_CONVEYOR || follow.state != PlayerState.GROUND)
            {
                _flickering = false;
                visible = false;
                return;
            }
            
            if (GlobalState.CURRENT_MAP_NAME == "WINDMILL") //only map that doesn't have the foot overlay flicker
            {
                visible = true;
            }
            else
            {
                Flicker(0.2f);
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            Position = follow.Position - new Vector2(7, 3);
            if (follow.facing == Facing.RIGHT)
            {
                Position.X--;
            }
        }
    }
}
