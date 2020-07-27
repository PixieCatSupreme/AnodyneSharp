using AnodyneSharp.Drawing;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Holes
{
    [NamedEntity, Collision(typeof(Player))]
    public class CrackedTile : Entity
    {
        Hole _hole;
        float _crackTimer = 1f;

        public CrackedTile(EntityPreset preset, Player p)
            : base(preset.Position, "crackedtiles", 16, 16, DrawOrder.MAP_BG2)
        {
            immovable = true;
            Solid = false;

            _hole = new Hole(Position, preset.Frame)
            {
                exists = false
            };

            SetFrame(preset.Frame);
        }
        public override void Collided(Entity other)
        {
            if (exists && other is Player player && player.state != PlayerState.AIR)
            {
                _crackTimer -= GameTimes.DeltaTime;
                if (_crackTimer < 0)
                {
                    SoundManager.PlaySoundEffect("floor_crack");
                    exists = false;
                    _hole.exists = true;
                }
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { _hole };
        }
    }
}
