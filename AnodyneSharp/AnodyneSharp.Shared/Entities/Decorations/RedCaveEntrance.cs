using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Decorations
{
    [Collision(PartOfMap = true)]
    public class RedCaveEntrance : Entity
    {
        public RedCaveEntrance(EntityPreset preset, int required_events) : base(preset.Position, "red_cave_left", 64,64,Drawing.DrawOrder.ENTITIES)
        {
            offset = new Vector2(4,32);
            Position += offset;
            width = 56;
            height = 28;

            immovable = true;

            if(GlobalState.events.GetEvent(preset.TypeValue) < required_events)
            {
                exists = false;
                GlobalState.SpawnEntity(new DoorToggle(Position,width,height));
            }
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }
    }

    [NamedEntity("Solid_Sprite", "red_cave_l_ss")]
    public class RedCaveLeft : RedCaveEntrance
    {
        public RedCaveLeft(EntityPreset preset, Player p) : base(preset, 1) { }
    }

    [NamedEntity("Solid_Sprite", "red_cave_r_ss")]
    public class RedCaveRight : RedCaveEntrance
    {
        public RedCaveRight(EntityPreset preset, Player p) : base(preset, 1) { }
    }

    [NamedEntity("Solid_Sprite", "red_cave_c_ss")]
    public class RedCaveCenter : RedCaveEntrance
    {
        public RedCaveCenter(EntityPreset preset, Player p) : base(preset, 0) { }
    }

    [NamedEntity("Solid_Sprite", "red_cave_n_ss")]
    public class RedCaveNorth : RedCaveEntrance
    {
        public RedCaveNorth(EntityPreset preset, Player p) : base(preset, 2) { }
    }
}
