using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
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

        public Eye_Light(EntityPreset preset, Player p) 
            : base(preset.Position, new AnimatedSpriteRenderer("eyelight",16,16, new Anim("glow",new int[] { 0, 1, 2 },5)), DrawOrder.ENTITIES )
        {
            light = new FiveFrameGlowLight(Position + new Vector2(8,6));
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(light, 1);
        }
    }

}
