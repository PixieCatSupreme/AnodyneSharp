using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    [NamedEntity("Event",null,1)]
    public class VolumeEvent : Entity
    {
        float target;
        public float speed;
        public VolumeEvent(EntityPreset preset, Player p) : this(float.Parse(preset.TypeValue))
        {
        }

        public VolumeEvent(float target, float speed = 0.4f) : base(Vector2.Zero)
        {
            SetTarget(target);
            this.speed = speed;
            visible = false;
        }

        public void SetTarget(float t)
        {
            target = t;
            ReachedTarget = false;
        }

        public bool ReachedTarget { get; private set; } = false;

        public override void Update()
        {
            if (!ReachedTarget)
            {
                //VolumeEvent only changes bgm volume
                float current = SoundManager.GetVolume();
                ReachedTarget = MathUtilities.MoveTo(ref current, target, speed);
                SoundManager.SetSongVolume(current);
            }
        }
    }
}
