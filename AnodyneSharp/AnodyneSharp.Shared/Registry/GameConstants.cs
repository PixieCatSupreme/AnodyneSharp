using AnodyneSharp.Dialogue;

namespace AnodyneSharp.Registry
{
    public static class GameConstants
    {
        public static int MAX_DOOR_PAIRS = 95;
        //public static var DOOR_REFS:Array = private new Array();
        //public static var DOOR_INFO:Array = private new Array(MAX_DOOR_PAIRS);
        public const int SCREEN_WIDTH_IN_TILES = 10;
        public const int SCREEN_HEIGHT_IN_TILES = 10;
        public const int TILE_WIDTH = 16;
        public const int TILE_HEIGHT = 16;
        public const int SCREEN_WIDTH_IN_PIXELS = 160;
        public const int SCREEN_HEIGHT_IN_PIXELS = 160;
        public const int HEADER_HEIGHT = 20; //Height of header. Needed in screen scrolling 
        public static string SAVE_NAME = "ANODYNE_SAVE_123";
	    public static object DIALOGUE;
        public static object DIALOGUE_STATE;
        //public static const DUNGEON_ENTRANCES:Object = { STREET: new Point(232, 737), BEDROOM: new Point(400, 765), REDCAVE: new Point(555, 764), CROWD: new Point(555, 1239), APARTMENT: new Point(391, 911), HOTEL: new Point(394, 440), CIRCUS: new Point(715, 1396), NEXUS: new Point(710,1090) };

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
