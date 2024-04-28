using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class BroomTreasure : Treasure
    {
        private BroomType _type;

        public BroomTreasure(string textureName, Vector2 pos, BroomType broomType)
            : base(textureName, pos, 0, -1)
        {
            _type = broomType;

            switch (broomType)
            {
                case BroomType.Normal:
                    _dialogueID = 1;
                    break;
                case BroomType.Wide:
                    _dialogueID = 4;
                    break;
                case BroomType.Long:
                    _dialogueID = 5;
                    break;
                case BroomType.Transformer:
                    _dialogueID = 6;
                    break;
            }
        }

        public override void GetTreasure()
        {
            base.GetTreasure();

            GlobalState.inventory.UnlockBroom(_type);
        }
    }
}
