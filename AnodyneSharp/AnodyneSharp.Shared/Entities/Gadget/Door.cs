using AnodyneSharp.Drawing;
using AnodyneSharp.Logging;
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

        public Door(EntityPreset preset, Player player)
            : this(preset, player, "doors", 16, 16)
        { }

        public Door(EntityPreset preset, Player player, string textureName, int width, int height)
            : base(preset.Position, textureName, width, height, DrawOrder.BG_ENTITIES)
        {
            _linkedDoor = EntityManager.GetLinkedDoor(preset);
            teleportOffset = Vector2.Zero;

            immovable = true;
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                TeleportPlayer();
            }
        }

        protected virtual void TeleportPlayer()
        {
            //TODO: Screen transition
            DebugLogger.AddInfo($"Teleporting player to map {_linkedDoor.Map} at {_linkedDoor.Door.Position.X}, {_linkedDoor.Door.Position.Y}. Door pair {_linkedDoor.Door.Frame}.");
        }
    }
}
