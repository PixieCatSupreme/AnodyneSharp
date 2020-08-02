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

        Player _player;

        public NexusPad(EntityPreset preset, Player player)
            : base(preset, player, "nexus_pad", 32, 32, null)
        {
            _player = player;

            width = 22;
            height = 18;
            offset = new Vector2(6,4);

            Position += new Vector2(6, 6);
            teleportOffset = new Vector2(10, 34);

            if (GlobalState.CURRENT_MAP_NAME == "CELL")
            {
                AddAnimation("on", CreateAnimFrameArray(3), 12, false);
                AddAnimation("off", CreateAnimFrameArray(2), 12, false);
            }
            else
            {
                AddAnimation("on", CreateAnimFrameArray(1), 12, false);
                AddAnimation("off", CreateAnimFrameArray(0), 12, false);
            }
            Play("off");

            GlobalState.ActivatedNexusPortals[MapUtilities.GetMapID(GlobalState.CURRENT_MAP_NAME)] = true;
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