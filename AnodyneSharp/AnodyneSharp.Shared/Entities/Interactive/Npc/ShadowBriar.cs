using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    public class ShadowBriar : Entity
    {
        protected EntityPreset preset;

        public ShadowBriar(EntityPreset preset, Player p)
            : base(preset.Position, "briar", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("idle_d", CreateAnimFrameArray(20), 12);
            AddAnimation("idle_l", CreateAnimFrameArray(26), 12);
            AddAnimation("idle_r", CreateAnimFrameArray(22), 12);
            AddAnimation("idle_u", CreateAnimFrameArray(24), 12);
            AddAnimation("walk_d", CreateAnimFrameArray(20, 21), 4);
            AddAnimation("walk_l", CreateAnimFrameArray(26, 27), 4);
            AddAnimation("walk_r", CreateAnimFrameArray(22, 23), 4);
            AddAnimation("walk_u", CreateAnimFrameArray(24, 25), 4);
            Play("idle_d");

            this.preset = preset;
        }
    }
}
