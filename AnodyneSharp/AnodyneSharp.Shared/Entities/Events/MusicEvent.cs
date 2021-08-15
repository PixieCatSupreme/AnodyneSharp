using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    [NamedEntity("Music_Event")]
    class MusicEvent : Entity
    {
        public MusicEvent(EntityPreset preset, Player p) : base(Vector2.Zero,Drawing.DrawOrder.BACKGROUND)
        {
            if(preset.Frame == 0 || GlobalState.events.GetEvent(preset.TypeValue) == 0)
            {
                Sounds.SoundManager.PlaySong(GlobalState.CURRENT_MAP_NAME.ToLower() + "_alt");
            }
            exists = false;
        }
    }
}
