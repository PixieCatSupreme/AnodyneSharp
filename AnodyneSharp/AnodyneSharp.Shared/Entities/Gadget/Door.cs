using AnodyneSharp.Drawing;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    public class Door : Entity
    {
        protected string LinkedMapName
        {
            get
            {
                return _linkedDoor.Map;
            }
        }

        private DoorMapPair _linkedDoor;

        protected Vector2 teleportOffset;

        private bool player_on_door;
        private Player _player;

        public Door(EntityPreset preset, Player player)
            : this(preset, player, "doors", 16, 16)
        { }

        public Door(EntityPreset preset, Player player, string textureName, int width, int height)
            : base(preset.Position, textureName, width, height, DrawOrder.BG_ENTITIES)
        {
            _linkedDoor = EntityManager.GetLinkedDoor(preset);
            teleportOffset = Vector2.Zero;

            immovable = true;

            _player = player;
            player_on_door = player.Hitbox.Intersects(this.Hitbox);
        }

        protected override void CenterOffset()
        {
            base.CenterOffset();
            player_on_door = _player.Hitbox.Intersects(this.Hitbox);
        }

        public override void Update()
        {
            base.Update();
            if(!_player.Hitbox.Intersects(this.Hitbox))
            {
                player_on_door = false;
            }
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                if(!player_on_door)
                    TeleportPlayer();
            }
        }

        protected virtual void TeleportPlayer()
        {
            DebugLogger.AddInfo($"Teleporting player to map {_linkedDoor.Map} at {_linkedDoor.Door.Position.X}, {_linkedDoor.Door.Position.Y}. Door pair {_linkedDoor.Door.Frame}.");
            GlobalState.NEXT_MAP_NAME = _linkedDoor.Map;
            GlobalState.PLAYER_WARP_TARGET = _linkedDoor.Door.Position + teleportOffset;
            GlobalState.WARP = true;
            player_on_door = true;
        }
    }
}
