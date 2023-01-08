using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.UI;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    public class PlayerDieDummy : UIEntity
    {
        public bool MovementDone { get; private set; }

        public PlayerDieDummy(Vector2 pos) 
            : base(MapUtilities.GetInGridPosition(pos), Player.GetSprite(GlobalState.IsCell), DrawOrder.PLAYER_DIE_DUMMY)
        {
            Position.Y += 20;

            Play("die");
        }

        public override void Update()
        {
            base.Update();

            if (MathUtilities.MoveTo(ref Position.X, 80, 30f) & MathUtilities.MoveTo(ref Position.Y, 100,30f))
            {
                MovementDone = true;
            }
        }
    }
}
