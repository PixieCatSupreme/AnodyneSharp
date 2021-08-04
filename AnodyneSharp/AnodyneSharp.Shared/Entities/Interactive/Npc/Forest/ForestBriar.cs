using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    [NamedEntity("Shadow_Briar", map: "FOREST"), Collision(typeof(Player))]
    public class ForestBriar : ShadowBriar
    {
        public ForestBriar(EntityPreset preset, Player p)
            : base(preset, p)
        {
            Play("walk_u");

            opacity = 0;

            velocity.Y = -20;
        }

        public override void Update()
        {
            base.Update();

            MathUtilities.MoveTo(ref opacity, 1, 0.3f);

            Rectangle screen = new(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid.ToPoint()).ToPoint(), new(160, 160));
            if (!screen.Intersects(Hitbox))
            {
                exists = false;
                preset.Alive = false;
            }
        }
    }
}
