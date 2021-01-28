using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    [NamedEntity("Event",null,1)]
    class VolumeEvent : Entity
    {
        float target;
        public VolumeEvent(EntityPreset preset, Player p) : base(preset.Position,Drawing.DrawOrder.ENTITIES)
        {
            visible = false;
            _ = float.TryParse(preset.TypeValue, out target);
        }

        public override void Update()
        {
            //VolumeEvent only changes bgm volume
            float current = SoundManager.GetVolume();
            MathUtilities.MoveTo(ref current, target, 0.3f);
            SoundManager.SetSongVolume(current);
        }
    }
}
