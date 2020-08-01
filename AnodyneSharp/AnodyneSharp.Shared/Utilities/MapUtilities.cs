using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using static AnodyneSharp.Registry.GameConstants;

namespace AnodyneSharp.Utilities
{
    public static class MapUtilities
    {
        public static Vector2 GetRoomCoordinate(Vector2 pos)
        {
            return new Vector2((int)(pos.X / SCREEN_WIDTH_IN_PIXELS), (int)(pos.Y / SCREEN_HEIGHT_IN_PIXELS));
        }

        public static Vector2 GetRoomUpperLeftPos(Vector2 pos)
        {
            return pos * SCREEN_WIDTH_IN_PIXELS;
        }

        public static Vector2 GetInGridPosition(Vector2 pos)
        {
            return pos - GetRoomUpperLeftPos(GetRoomCoordinate(pos));
        }

        public static int GetMapID(string mapName)
        {
            return (int)Enum.Parse(typeof(MapOrder), mapName);
        }
    }
}
