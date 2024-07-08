using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Enemy.Hotel.Boss
{
    [Enemy]
    public class EnemyMarker : Entity
    {
        public EnemyMarker() : base(Vector2.Zero) {
            visible = false;
        }
    }
}
