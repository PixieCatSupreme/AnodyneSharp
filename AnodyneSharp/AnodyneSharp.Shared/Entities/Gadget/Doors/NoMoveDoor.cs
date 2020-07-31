using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "9"), Collision(typeof(Player))]
    public class NoMoveDoor : Door
    {
        public NoMoveDoor(EntityPreset preset, Player player)
            : base(preset, player)
        { }

        public override void Draw()
        { }

        protected override void TeleportPlayer()
        {
            //TODO: Enable when teleporting is a thing. (We don't want to listen to it otherwise)
            //SoundManager.PlaySoundEffect("enter_Door");
            base.TeleportPlayer();
        }
    }
}
