using AnodyneSharp.Entities.Interactive.Npc;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "7"), Collision(typeof(Player))]
    public class WhirlPool : Door
    {
        private EntityPreset _preset;

        public WhirlPool(EntityPreset preset, Player player)
            : base(preset, player, "whirlpool", 16, 16, null)
        {
            AddAnimation("whirl", CreateAnimFrameArray(0, 1), 6, true);
            AddAnimation("transition", CreateAnimFrameArray(3, 4, 4), 6, false);
            AddAnimation("whirl_red", CreateAnimFrameArray(4, 5), 6, true);

            _preset = preset;

            if (GlobalState.events.GetEvent("fisherman.dead") != 0)
            {
                Play("whirl_red");
            }
            else
            {
                Play("whirl");
            }

            if (GlobalState.CURRENT_MAP_NAME == "REDSEA")
            {
                teleportOffset = new Vector2(0, -36);
            }
        }

        public override void Update()
        {
            base.Update();

            if (CurAnimFinished)
            {
                Play("whirl_red");
            }
        }

        public void DoTransition()
        {
            GlobalState.events.IncEvent("fisherman.dead");
            Play("transition");
        }
    }
}
