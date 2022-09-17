using System;
using AnodyneSharp.Dialogue;

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

        static GameConstants()
        {
            SavePath =
#if DEBUG
                "./";
#else
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My games/AnodyneFanRemake/";
#endif


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
