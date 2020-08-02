using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Doors
{
    [Collision(typeof(Player))]
    public class DirectionalDoor : Door
    {
        Facing _exitDirection;
        Player _player;

        public DirectionalDoor(EntityPreset preset, Player player, Facing facing, int offsetSize)
            : base(preset, player)
        {
            visible = false;

            teleportOffset = FacingDirection(facing) * offsetSize;

            _exitDirection = facing;

            _player = player;
        }

        protected override void TeleportPlayer()
        {
            //TODO: Enable when teleporting is a thing. (We don't want to listen to it otherwise)
            //SoundManager.PlaySoundEffect("enter_Door");

            _player.facing = _exitDirection;

            base.TeleportPlayer();
        }
    }

    [NamedEntity("Door", "1")]
    public class DownDoor : DirectionalDoor
    {
        public DownDoor(EntityPreset preset, Player player)
            : base(preset, player, Facing.DOWN, 16)
        { }
    }

    [NamedEntity("Door", "5")]
    public class UpDoor : DirectionalDoor
    {
        public UpDoor(EntityPreset preset, Player player)
            : base(preset, player, Facing.UP, 20)
        {
            width = height = 8;
            CenterOffset();
        }
    }

    [NamedEntity("Door", "10")]
    public class LeftDoor : DirectionalDoor
    {
        public LeftDoor(EntityPreset preset, Player player)
            : base(preset, player, Facing.LEFT, 16)
        { }
    }

    [NamedEntity("Door", "11")]
    public class RightDoor : DirectionalDoor
    {
        public RightDoor(EntityPreset preset, Player player)
            : base(preset, player, Facing.RIGHT, 17)
        { }
    }

    [NamedEntity("Door", "12")]
    public class WideDownDoor : DirectionalDoor
    {
        public WideDownDoor(EntityPreset preset, Player player)
            : base(preset, player, Facing.DOWN, 16)
        {
            width = 80;
            Position.X -= 32;
        }
    }

    [NamedEntity("Door", "13")]
    public class TallLeftDoor : LeftDoor
    {
        public TallLeftDoor(EntityPreset preset, Player player)
            : base(preset, player)
        {
            height = 80;
            Position.Y -= 32;
        }
    }

    [NamedEntity("Door", "14")]
    public class WideUpDoor : DirectionalDoor
    {
        public WideUpDoor(EntityPreset preset, Player player)
            : base(preset, player, Facing.UP, 20)
        {
            width = 80;
            Position.X -= 32;
        }
    }

    [NamedEntity("Door", "15")]
    public class TallRightDoor : RightDoor
    {
        public TallRightDoor(EntityPreset preset, Player player)
            : base(preset, player)
        {
            height = 80;
            Position.Y -= 32;
        }
    }
}
