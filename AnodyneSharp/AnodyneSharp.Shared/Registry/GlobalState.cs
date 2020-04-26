
using AnodyneSharp.Entities;
using System;

namespace AnodyneSharp.Registry
{
    public static class GlobalState
    {
        public static bool BOI = false; // BOI Sprites on in easter egg redcave
        public static bool is_playstate = true;
        public static double volume_scale = 1.0;
        public static bool pillar_switch_state = false; //Reset when entering a map with differentname
        public static int ENTRANCE_GRID_X;
        public static int ENTRANCE_GRID_Y;
        public static int ENTRANCE_PLAYER_X;
        public static int ENTRANCE_PLAYER_Y;
        public static int PLAYER_X;
        public static int PLAYER_Y;
        public static int CURRENT_GRID_X;
        public static int CURRENT_GRID_Y;
        public static string CURRENT_MAP_NAME;

        public static bool autosave_on = true;
        //public static var checkpoint:Object = { x: 0, y: 0, area: "" };
        public static string cur_dialogue;
        /**
         * Used for disabling the menu during an event because you could potentially break the game  otherwise
         */
        public static bool disable_menu = false;

        /* Grid-local state that needs to be accessed globally for events */
        public static int GRID_ENEMIES_DEAD = 0;
        public static int GRID_PUZZLES_DONE = 0;

        public static bool FUCK_IT_MODE_ON = false;

        //Health stuff
        public static int CUR_HEALTH;
        public static int MAX_HEALTH;

        public static Random RNG = new Random();

        //Cool new debug values
#if DEBUG
        public static bool DrawBG = true;
        public static bool DrawBG2 = true;
        public static bool DrawFG = true;

        public static bool MovingCamera = true;
        public static bool FreeRoamCamera = false;
#endif
    }
}
