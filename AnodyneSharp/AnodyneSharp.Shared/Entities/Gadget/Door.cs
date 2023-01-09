using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

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
        protected Player _player;

        private string? _sfx;

        //Here to be disabled/re-enabled as necessary for eg REDSEA->REDCAVE and CROWD falling door
        public bool Active = true;

        public Door(EntityPreset preset, Player player, string? sfx = "enter_door")
            : base(preset.Position,16,16)
        {
            _linkedDoor = EntityManager.GetLinkedDoor(preset);
            teleportOffset = Vector2.Zero;

            immovable = true;

            _player = player;
            player_on_door = player.Hitbox.Intersects(Hitbox);

            _sfx = sfx;
        }

        public Door(EntityPreset preset, Player p, AnimatedSpriteRenderer sprite, string? sfx = "enter_door") : base(preset.Position, sprite, DrawOrder.BG_ENTITIES)
        {
            _linkedDoor = EntityManager.GetLinkedDoor(preset);
            teleportOffset = Vector2.Zero;

            immovable = true;

            _player = p;
            player_on_door = p.Hitbox.Intersects(Hitbox);

            _sfx = sfx;
        }

        protected override void CenterOffset(bool updatePos = true)
        {
            base.CenterOffset(updatePos);
            player_on_door = _player.Hitbox.Intersects(Hitbox);
        }

        public override void Update()
        {
            base.Update();
            if(!_player.Hitbox.Intersects(Hitbox))
            {
                player_on_door = false;
            }
        }

        public override void Collided(Entity other)
        {
            if (other is Player && Active)
            {
                if(!player_on_door)
                    TeleportPlayer();
            }
        }

        protected virtual void TeleportPlayer()
        {
            DebugLogger.AddInfo($"Teleporting player to map {_linkedDoor.Map} at {_linkedDoor.Door.Position.X}, {_linkedDoor.Door.Position.Y}. Door pair {_linkedDoor.Door.Frame}.");
            _linkedDoor.Warp(teleportOffset);
            player_on_door = true;

            if (_sfx != null)
            {
                SoundManager.PlaySoundEffect(_sfx);
            }
        }
    }
}
