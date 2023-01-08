using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    public class ShadowBriar : Entity
    {
        protected EntityPreset preset;

        public static AnimatedSpriteRenderer GetSprite() => new("briar", 16, 16,
            new Anim("idle_d", new int[] { 20 }, 12),
            new Anim("idle_l", new int[] { 26 }, 12),
            new Anim("idle_r", new int[] { 22 }, 12),
            new Anim("idle_u", new int[] { 24 }, 12),
            new Anim("walk_d", new int[] { 20, 21 }, 4),
            new Anim("walk_l", new int[] { 26, 27 }, 4),
            new Anim("walk_r", new int[] { 22, 23 }, 4),
            new Anim("walk_u", new int[] { 24, 25 }, 4)
            );

        public ShadowBriar(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            this.preset = preset;
        }
    }
}
