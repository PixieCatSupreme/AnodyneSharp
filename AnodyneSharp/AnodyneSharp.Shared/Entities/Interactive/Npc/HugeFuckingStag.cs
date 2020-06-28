using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("Huge_Fucking_Stag")]
    public class HugeFuckingStag : Entity
    {
        public HugeFuckingStag(EntityPreset preset)
           : base(preset.Position, "forest_stag", 64, 80, DrawOrder.FG_SPRITES)
        {
            AddAnimation("a", CreateAnimFrameArray(0, 1, 2), 4);

            Play("a");

            Random rng = new Random();

            exists = rng.Next(0, 100) == 0 || (KeyInput.IsKeyPressed(Keys.Q) && KeyInput.IsKeyPressed(Keys.W) && KeyInput.IsKeyPressed(Keys.E));
        }
    }
}
