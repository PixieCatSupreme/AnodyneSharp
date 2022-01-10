using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Hotel.Boss
{
    [NamedEntity("Eye_Boss",null,4), Collision(typeof(Player),typeof(Broom),MapCollision = true)]
    public class LandPhase : Entity
    {
        EnemyMarker death_marker = new();
        public LandPhase(EntityPreset preset, Player p) : base(preset.Position, "eye_boss_water", 24, 24, Drawing.DrawOrder.ENTITIES)
        {
            EntityPreset water = EntityManager.GetLinkGroup(preset.LinkID).Where(e => e != preset).First();
            exists = !water.Alive;
            if (!exists) return;
            (GlobalState.Map as MapData.Map).IgnoreMusicNextUpdate(); //Make sure music doesn't change back if player moves back up
            SoundManager.PlaySong("hotel-boss"); //in case player returned after warping to entrance
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { death_marker };
        }
    }
}
