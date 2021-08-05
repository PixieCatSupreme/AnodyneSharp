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
        bool smack_on_next_screen = true;

        public FallDoor(EntityPreset preset, Player player)
            : base(preset, player)
        {
            visible = false;
        }

        public override void Collided(Entity other)
        {
            if(other is Player p && Active)
            {
                if (p.state == PlayerState.GROUND || (p.state == PlayerState.ENTER_FALL && p.offset.Y == 0))
                {
                    TeleportPlayer();
                    p.enter_fall = true;
                    if (smack_on_next_screen)
                    {
                        p.fall_smack = true;
                        //prevent fall_smack from acting before the map transition is complete
                        p.state = PlayerState.GROUND;
                        p.dontMove = true;
                    }
                    Active = false;
                }
                else if(p.state == PlayerState.AUTO_JUMP)
                {
                    //First jump starts with an autojump onto the top floor door
                    //Only the second door needs to have the player land with a smack
                    smack_on_next_screen = false;
                }
            }
        }

        protected override void TeleportPlayer()
        {
            base.TeleportPlayer();
        }
    }
}
