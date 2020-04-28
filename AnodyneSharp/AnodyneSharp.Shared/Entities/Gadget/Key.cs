using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity, Collision(typeof(Player))]
    public class Key : Entity
    {
        EntityPreset _preset;

        public Key(EntityPreset preset) 
            : base(preset.Position, 16, 16, DrawOrder.ENTITIES)
        {
            _preset = preset;

            SetTexture("key");
            SetFrame(_preset.Frame);
        }

        public override void Collided(Entity other)
        {
            if (visible)
            {
                visible = false;
                InventoryState.AddCurrentMapKey();
                SoundManager.PlaySoundEffect("keyget");
                _preset.Alive = false;
            }
        }
    }
}
