﻿
using AnodyneSharp.Entities;
using System;

namespace AnodyneSharp.Registry
{
    public enum GameMode
    {
        Normal,
        Chaos,
        EXTREME_CHAOS
    }

    public static class GlobalState
    {
        public static DateTime START_TIME;

        public static string Dialogue
        {
            get
            {
                return _dialogue;
            }
            set
            {
                _dialogue = value;
                SetDialogueMode = value != "";
            }
        }

        public static bool SetDialogueMode = false;

        public static bool BOI = false; // BOI Sprites on in easter egg redcave
        public static bool is_playstate = true;
        public static float music_volume_scale =
#if DEBUG
            0.1f;
#else
            1.0f;
#endif
        public static float sfx_volume_scale = 1.0f;

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

        public static bool RefreshKeyCount = false;
        public static bool RefreshMaxHealth = false;

        public static bool autosave_on = true;
        //public static var checkpoint:Object = { x: 0, y: 0, area: "" };

        /**
         * Used for disabling the menu during an event because you could potentially break the game  otherwise
         */
        public static bool disable_menu = false;

        /* Grid-local state that needs to be accessed globally for events */
        public static int GRID_ENEMIES_DEAD = 0;
        public static int GRID_PUZZLES_DONE = 0;

        public static bool FUCK_IT_MODE_ON = false;

        //Health stuff
        public static int CUR_HEALTH = 6;
        public static int MAX_HEALTH = 6;

        //Drawing stuff
        public static float UI_SCALE;

        //Cheatz stuff
        public static bool AlwaysCellGraphics = false;
        public static GameMode GameMode = GameMode.Normal;
        public static bool ForceTextureReload = false;

        public static Random RNG = new Random();

        public static bool ShowFPS =
#if DEBUG
            true;
#else
            false;
#endif

        //Cool new debug values
#if DEBUG
        public static bool DrawBG = true;
        public static bool DrawBG2 = true;
        public static bool DrawFG = true;

        public static bool MovingCamera = true;
        public static bool FreeRoamCamera = false;
#endif

        //Song deciding bools
        public static bool OnRoof;
        public static bool InDeathRoom;
        public static bool HappyStarted;
        public static bool SageDead;
        public static bool SuburbSoft;
        public static bool WindmillOpened;

        private static string _dialogue;
    }
}
