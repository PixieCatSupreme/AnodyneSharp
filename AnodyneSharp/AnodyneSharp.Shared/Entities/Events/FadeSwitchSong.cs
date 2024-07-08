using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    public class FadeSwitchSong : Entity
    {
        VolumeEvent volumeChanger = new(0, 1.6f);
        string next;

        public FadeSwitchSong(string nextSong) : base(Vector2.Zero)
        {
            visible = false;
            next = nextSong;
        }

        public override void Update()
        {
            if(volumeChanger.ReachedTarget)
            {
                SoundManager.PlaySong(next, 0);
                volumeChanger.SetTarget(1);
                exists = false;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(volumeChanger, 1);
        }
    }
}
