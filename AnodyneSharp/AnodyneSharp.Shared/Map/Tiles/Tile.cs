namespace AnodyneSharp.Map.Tiles
{
    internal class Tile
    {
        internal bool visible;

        private TileMap tileMap;
        private int index;
        private int _tileWidth;
        private int _tileHeight;
        private bool v;
        private CollissionDirections collissionDirections;

        public Tile(TileMap tileMap, int i, int tileWidth, int tileHeight, bool v, CollissionDirections collissionDirections)
        {
            visible = true;
            this.tileMap = tileMap;
            index = i;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            this.v = v;
            this.collissionDirections = collissionDirections;
        }
    }
}