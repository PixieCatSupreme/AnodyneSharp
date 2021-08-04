using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Forest
{
    [NamedEntity("Shadow_Briar", map: "BEACH"), Collision(typeof(Player))]
    public class BeachBriar : ShadowBriar
    {
        public BeachBriar(EntityPreset preset, Player p) 
            : base(preset, p)
        {
            Play("walk_d");

            opacity = 0;

            velocity.Y = 20;
        }

        public override void Update()
        {
            base.Update();

            if (preset.Alive)
            {
                if (MathUtilities.MoveTo(ref opacity, 1, 0.42f))
                {
                    velocity.Y = 0;
                    Play("idle_d");
                }        
            }
            else
            {
                if (MathUtilities.MoveTo(ref opacity, 0, 0.6f))
                {
                    exists = false;
                }
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (velocity.Y == 0)
            {
                velocity.Y = 15;

                preset.Alive = false;

                Play("walk_d");
            }
        }
    }
}
