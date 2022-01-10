using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Enemy.Hotel.Boss
{
    [Enemy]
    class EnemyMarker : Entity
    {
        public EnemyMarker() : base(Vector2.Zero, Drawing.DrawOrder.BACKGROUND) {
            visible = false;
        }
    }
}
