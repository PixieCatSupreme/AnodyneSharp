using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity, Collision(typeof(Player))]
    public class Key : Entity
    {
        EntityPreset _preset;

        public Key(EntityPreset preset, Player p)
            : base(preset.Position, new StaticSpriteRenderer("key", 16, 16, preset.Frame), DrawOrder.ENTITIES)
        {
            _preset = preset;
        }

        public override void Collided(Entity other)
        {
            if (visible)
            {
                visible = false;
                GlobalState.inventory.AddCurrentMapKey();
                SoundManager.PlaySoundEffect("keyget");
                _preset.Alive = false;
            }
        }
    }
}
