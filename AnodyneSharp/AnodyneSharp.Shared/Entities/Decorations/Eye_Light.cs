using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Lights;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Entities.Decorations
{
    [NamedEntity]
    public class Eye_Light : Entity
    {
        FiveFrameGlowLight light;

        public Eye_Light(EntityPreset preset, Player p) : base(preset.Position, "eyelight", 16, 16, DrawOrder.ENTITIES)
        {
            AddAnimation("glow", CreateAnimFrameArray(0, 1, 2), 5);
            Play("glow");

            light = new FiveFrameGlowLight(Position + new Vector2(8,6));
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(light, 1);
        }
    }

}
