using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class Treasure : BaseTreasure
    {
        private Vector2 _endPos;

        public Treasure(string textureName, Vector2 pos, int frame, int dialogueID)
            : base(textureName, pos, frame, dialogueID)
        {
            _endPos = new Vector2(pos.X, pos.Y - 16);
            exists = false;
        }

        public override void Update()
        {
            base.Update();

            if (_flickering)
            {
                if (Position.Y > _endPos.Y)
                {
                    Position.Y = Math.Max(Position.Y - 25 * GameTimes.DeltaTime, _endPos.Y);
                }
            }
            else
            {
                exists = false;
            }

            base.PostUpdate();
        }

        public override void GetTreasure()
        {
            base.GetTreasure();

            Flicker(1);
            exists = true;
        }
    }
}
