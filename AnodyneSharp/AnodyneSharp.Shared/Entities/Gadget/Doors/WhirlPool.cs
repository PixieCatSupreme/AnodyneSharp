using AnodyneSharp.Entities.Base.Rendering;
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

        public static AnimatedSpriteRenderer GetSprite() => new("whirlpool", 16, 16,
            new Anim("whirl", new int[] { 0, 1 }, 6),
            new Anim("transition", new int[] { 3, 4, 4 },6,false),
            new Anim("whirl_red", new int[] { 4, 5 },6)
            );

        public WhirlPool(EntityPreset preset, Player player)
            : base(preset, player, GetSprite(), null)
        {
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

            if (AnimFinished)
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
