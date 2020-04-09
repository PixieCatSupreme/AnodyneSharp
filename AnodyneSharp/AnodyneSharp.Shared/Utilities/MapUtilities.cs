using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;

using static AnodyneSharp.Registry.GameConstants;

namespace AnodyneSharp.Utilities
{
    public static class MapUtilities
    {
        public static Vector2 GetRoomCoordinate(Vector2 pos)
        {
            return pos / SCREEN_WIDTH_IN_PIXELS;
        }

        public static Vector2 GetRoomUpperLeftPos(Vector2 pos)
        {
            return pos * SCREEN_WIDTH_IN_PIXELS;
        }

        public static Vector2 GetCamRoomPos(Vector2 pos)
        {
            return AddCamOffset(pos * SCREEN_WIDTH_IN_PIXELS);
        }

        public static Vector2 AddCamOffset(Vector2 pos)
        {
            return new Vector2(pos.X, pos.Y - HEADER_HEIGHT - TILE_HEIGHT * 2 + 2);
        }
    }
}
