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
        public bool AnimDone => _curAnim.Finished;

        public bool MovementDone { get; private set; }

        public PlayerDieDummy(Vector2 pos, string textureName) 
            : base(MapUtilities.GetInGridPosition(pos), textureName, 16,16, DrawOrder.PLAYER_DIE_DUMMY)
        {
            Position.Y += 20;
            AddAnimation("die", CreateAnimFrameArray(25, 26, 27, 24, 25, 26, 27, 24, 25, 26, 27, 32), 12, false);

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
