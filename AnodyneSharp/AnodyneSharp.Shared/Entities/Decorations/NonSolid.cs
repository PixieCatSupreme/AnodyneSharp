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
					SetTexture("grass_1");
					frameWidth = 16;
					frameHeight = 6;

					AddAnimation("whatever", CreateAnimFrameArray(0, 1), 10);
					Play("whatever");
					break;
				case "Grass_REDSEA":
					SetTexture("grass_REDSEA");
					frameWidth = 16;
					frameHeight = 16;
					break;
				case "Rail_1":
					SetTexture("rail");
					frameWidth = 16;
					frameHeight = 16;
					break;
				case "Rail_NEXUS":
					SetTexture("rail_NEXUS");
					frameWidth = 16;
					frameHeight = 17;
					break;
				case "Rail_CROWD":
					SetTexture("rail_CROWD");
					frameWidth = 16;
					frameHeight = 22;

					SetFrame(0);
					break;
			}

			Position = preset.Position;
		}

        public override void Draw()
        {
            base.Draw();
        }
    }
}
