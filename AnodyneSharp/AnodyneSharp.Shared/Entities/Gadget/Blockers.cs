using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    //This "entity" in the original just sets the tile it's on to tile index 1 without updating the graphics to get it to be a solid tile
    //We're making it a proper entity that acts like it's a tile
    [NamedEntity("solid_tile"), Collision(PartOfMap = true)]
    public class SolidTile : Entity
    {
        public SolidTile(EntityPreset preset, Player p) : base(preset.Position,16,16)
        {
            visible = false;
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }

    //The following entities can't use the regular collision system
    //They're used to stop the player from being able to wiggle glitch to the post-game Nexus area
    [NamedEntity("Solid_Sprite",type:"blocker")]
    public class Blocker : Entity
    {
        Player _player;
        public Blocker(EntityPreset preset, Player p) : base(preset.Position,64,4)
        {
            _player = p;
            visible = false;
            immovable = true;
            HasVisibleHitbox = true;
        }

        public override void Update()
        {
            base.Update();
            if(_player.Hitbox.Intersects(Hitbox))
            {
                if(_player.velocity.Y > 0)
                {
                    _player.Position.Y = Position.Y - _player.height;
                }
                else
                {
                    _player.Position.Y = Position.Y + height;
                }
            }
        }
    }

    [NamedEntity("Solid_Sprite", type: "vblock")]
    public class VerticalBlocker : Entity
    {
        Player _player;
        public VerticalBlocker(EntityPreset preset, Player p) : base(preset.Position, 4, 16)
        {
            _player = p;
            visible = false;
            immovable = true;
            HasVisibleHitbox = true;
        }

        public override void Update()
        {
            base.Update();
            if (_player.Hitbox.Intersects(Hitbox))
            {
                if (_player.velocity.X > 0)
                {
                    _player.Position.X = Position.X - _player.width;
                }
                else
                {
                    _player.Position.X = Position.X + width;
                }
            }
        }
    }
}
