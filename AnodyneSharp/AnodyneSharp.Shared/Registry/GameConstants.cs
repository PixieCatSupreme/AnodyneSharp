using System;
using System.Collections.Generic;
using System.IO;
using AnodyneSharp.Archipelago;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Resources.Loading;

namespace AnodyneSharp.Registry
{
    public static class GameConstants
    {
        public enum MapOrder
        {
            STREET,
            OVERWORLD,
            REDCAVE,
            CROWD,
            APARTMENT,
            HOTEL,
            CIRCUS,
            CLIFF,
            FOREST,
            WINDMILL,
            REDSEA,
            BEACH,
            BEDROOM,
            FIELDS,
            GO,
            TERMINAL,
            HAPPY,
            SPACE,
            CELL,
            SUBURB,
            BLUE,
            //All maps before this have nexus gates and are in order of nexus gate preview sprite
            NEXUS,
            BLANK,
            DRAWER,
            DEBUG
        };

        public static string SavePath;

        public const int SCREEN_WIDTH_IN_TILES = 10;
        public const int SCREEN_HEIGHT_IN_TILES = 10;
        public const int TILE_WIDTH = 16;
        public const int TILE_HEIGHT = 16;
        public const int SCREEN_WIDTH_IN_PIXELS = 160;
        public const int SCREEN_HEIGHT_IN_PIXELS = 160;
        public const int HEADER_HEIGHT = 20; //Height of header. Needed in screen scrolling 

        public const int BUTTON_WIDTH = 13;
        public const int BUTTON_HEIGHT = 14;

        public const int LOCATION_START_ID = 20130204;

        static GameConstants()
        {
            SavePath = "./";
        }

        public static int FONT_LINE_HEIGHT
        {
            get
            {
                if (GlobalState.CurrentLanguage == Language.ZH_CN)
                {
                    return 12;
                }
                else
                {
                    return 8;
                }
            }
        }

        public static int LineOffset
        {
            get
            {
                return FONT_LINE_HEIGHT - 8;
            }
        }
    }
}
