using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "4"), Collision(typeof(Player))]
    public class BlankPortal : Door
    {
        public BlankPortal(EntityPreset preset, Player player) 
            : base(preset, player, "whiteportal", 16, 16)
        {
            width = height = 2;

            if (GlobalState.CURRENT_MAP_NAME == "CELL")
            {
                AddAnimation("a", CreateAnimFrameArray(4,5),8,true);
            }
            else
            {
                AddAnimation("a", CreateAnimFrameArray(0,1,2), 8, true);
            }
            Play("a");
        }

        protected override void TeleportPlayer()
        {
            //TODO: Enable when teleporting is a thing. (We don't want to listen to it otherwise)
            //SoundManager.PlaySoundEffect("enter_Door");
            base.TeleportPlayer();
        }
    }
}
