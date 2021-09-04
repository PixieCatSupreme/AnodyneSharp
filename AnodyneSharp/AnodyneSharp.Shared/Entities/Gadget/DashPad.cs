using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("Dash_Pad"), Collision(typeof(Player))]
    class DashPad : Entity
    {
        float disabled_t = 0;

        public DashPad(EntityPreset preset, Player p) : base(preset.Position, "dash_pads", 16, 16, Drawing.DrawOrder.BG_ENTITIES)
        {
            SetFrame(preset.Frame + 4);
            facing = preset.Frame switch
            {
                0 => Facing.UP,
                1 => Facing.RIGHT,
                2 => Facing.DOWN,
                _ => Facing.LEFT
            };
        }

        public override void Update()
        {
            base.Update();
            disabled_t += GameTimes.DeltaTime;
            if(disabled_t > 1)
            {
                opacity = 1f;
                disabled_t = 0f;
            }
        }

        public override void Collided(Entity other)
        {
            if(opacity == 1 && (other as Player).state == PlayerState.GROUND && Hitbox.Contains(other.Center))
            {
                opacity = 0.5f;
                (other as Player).Dash(facing);
            }
        }
    }
}
