using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [NamedEntity("Door", "16"), Collision(typeof(Player))]
    public class NexusPad : Door, Interactable
    {
        bool playerCollission;

        public static AnimatedSpriteRenderer GetSprite(int off_frame) => new("nexus_pad", 32, 32,
                new Anim("off",new int[] { off_frame },1),
                new Anim("on", new int[] { off_frame + 1 },1)
            );

        public NexusPad(EntityPreset preset, Player player)
            : base(preset, player, GetSprite(GlobalState.IsCell ? 2 : 0), null)
        {
            width = 22;
            height = 18;
            offset = new Vector2(6,4);

            Position += new Vector2(6, 6);
            teleportOffset = new Vector2(10, 34);

            GlobalState.events.ActivatedNexusPortals.Add(GlobalState.CURRENT_MAP_NAME);
        }

        public override void Update()
        {
            base.Update();

            if (playerCollission)
            {
                if (!_player.Hitbox.Intersects(Hitbox))
                {
                    playerCollission = false;
                    Play("off");
                }
            }
        }

        public override void Collided(Entity other)
        {
            if (!playerCollission && other is Player p && p.state == PlayerState.GROUND)
            {
                Play("on");
                SoundManager.PlaySoundEffect("menu_select");

                playerCollission = true;
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            TeleportPlayer();

            return true;
        }
    }
}