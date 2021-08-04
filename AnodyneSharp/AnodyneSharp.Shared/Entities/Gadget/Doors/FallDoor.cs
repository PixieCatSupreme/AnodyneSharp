using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    //The logic in this door is very specific to the two FallDoors in CROWD, they don't appear anywhere else in the game
    //The two doors are in sequence, the second door sets the player up to hit the ground hard on the last location in the warp sequence
    [NamedEntity("Door", "8"), Collision(typeof(Player))]
    public class FallDoor : Door
    {
        public FallDoor(EntityPreset preset, Player player)
            : base(preset, player)
        {
            visible = false;
        }

        public override void Collided(Entity other)
        {
            if(other is Player p && (p.state == PlayerState.GROUND || (p.state == PlayerState.ENTER_FALL && p.offset.Y == 0)) && Active)
            {
                TeleportPlayer();
                p.enter_fall = true;
                if(p.state == PlayerState.ENTER_FALL)
                {
                    p.fall_smack = true;
                    //prevent fall_smack from acting before the map transition is complete
                    p.state = PlayerState.GROUND;
                    p.dontMove = true;
                }
                Active = false;
            }
        }

        protected override void TeleportPlayer()
        {
            base.TeleportPlayer();
        }
    }
}
