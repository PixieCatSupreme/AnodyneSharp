using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    [NamedEntity("NPC", "biofilm"), Collision(typeof(Player))]
    class BioFilm : Entity
    {
        EntityPreset _preset;

        public BioFilm(EntityPreset preset, Player p) : base(preset.Position, "biofilm", 32, 32, Drawing.DrawOrder.BG_ENTITIES)
        {
            _preset = preset;
            if(!preset.Activated)
            {
                GlobalState.SpawnEntity(new DoorToggle(Position, 32, 32));
                SetFrame(0);
            }
            else
            {
                SetFrame(1);
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if(!_preset.Activated && ((Player)other).state == PlayerState.ENTER_FALL && other.offset.Y < 16)
            {
                _preset.Activated = true;
                SetFrame(1);
                Sounds.SoundManager.PlaySoundEffect("broom_hit");
                GlobalState.SpawnEntity(new DoorToggle(Position, 32, 32));
            }
        }
    }
}
