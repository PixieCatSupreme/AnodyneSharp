using AnodyneSharp.Entities;

namespace AnodyneSharp.Map.Tiles
{
    public class Tile : GameObject
    {
        public CollisionEventType collisionEventType = CollisionEventType.NONE;
        public Touching direction = Touching.ANY;

        public Tile(int tileWidth, int tileHeight, bool visible, Touching collisionDirections)
            :base(tileWidth, tileHeight)
        {
            allowCollisions = collisionDirections;
            immovable = true;
            this.visible = visible;
        }
    }
}