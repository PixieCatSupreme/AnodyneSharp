using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Decorations
{
    [NamedEntity("Nonsolid")]
    class NonSolid : Entity
    {
		public NonSolid(EntityPreset preset, Player p)
			: base(preset.Position, Drawing.DrawOrder.FG_SPRITES)
		{
			switch (preset.TypeValue)
			{
				case "Grass_1":
					SetTexture("grass_1",16,6);
					
					AddAnimation("whatever", CreateAnimFrameArray(0, 1), 10);
					Play("whatever");
					break;
				case "Grass_REDSEA":
					SetTexture("grass_REDSEA", 16, 16);
					break;
				case "Rail_1":
					SetTexture("rail", 16, 16);
					break;
				case "Rail_NEXUS":
					SetTexture("rail_NEXUS", 16, 17);
					break;
				case "Rail_CROWD":
					SetTexture("rail_CROWD", 16, 22);
					break;
			}

			SetFrame(0);

			Position = preset.Position;
		}
    }
}
