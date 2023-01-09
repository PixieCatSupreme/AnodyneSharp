using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
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
        public HugeFuckingStag(EntityPreset preset, Player p)
           : base(preset.Position, new AnimatedSpriteRenderer("forest_stag", 64, 80, new Anim("a",new int[] { 0, 1, 2 },4)), DrawOrder.FG_SPRITES)
        {
            exists = GlobalState.RNG.Next(0, 100) == 0 || (KeyInput.IsKeyPressed(Keys.Q) && KeyInput.IsKeyPressed(Keys.W) && KeyInput.IsKeyPressed(Keys.E));
        }
    }
}
