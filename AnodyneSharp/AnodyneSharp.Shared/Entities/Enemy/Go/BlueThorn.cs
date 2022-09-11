using Microsoft.Xna.Framework;
using System.Collections;

namespace AnodyneSharp.Entities.Enemy.Go
{
    class BlueThorn : BigThorn
    {
        public BlueThorn(Vector2 pos) : base(pos, "briar_arm_right")
        {
            AddAnimation("off", CreateAnimFrameArray(1, 2, 3, 0), 4);
            Play("off");
            offset.X = 40;
            Position.X += offset.X;
        }

        public override IEnumerator GetAttacked(BigThorn blue, Player player)
        {
            throw new System.NotImplementedException();
        }
    }
}
