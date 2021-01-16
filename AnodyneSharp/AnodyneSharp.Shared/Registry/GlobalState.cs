
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing.Effects;
using AnodyneSharp.Entities;
using AnodyneSharp.Map;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
                if (value == "") DialogueTop = false;
            }
        }
        public static bool DialogueTop = false;
        public static bool DialogueJustFinished = false;
        public static bool SetDialogueMode = false;

        public static Vector2 CurrentMapGrid
        {
            get
            {
                return new Vector2(CURRENT_GRID_X, CURRENT_GRID_Y);
            }
        }

        public static bool CanChangeBroom
        {
            get
            {
                return !UseCellBroom && CURRENT_MAP_NAME != "SUBURB";
            }
        }

        public static bool UseCellBroom
        {
            get
            {
                return AlwaysCellGraphics || CURRENT_MAP_NAME == "CELL";
            }
        }

        public static Language CurrentLanguage { get; set; }
        public static bool FishermanDead { get; set; }

        public static float music_volume_scale =
#if DEBUG
            0.1f;
#else
            1.0f;
#endif
        public static float sfx_volume_scale = 1.0f;

        public static bool pillar_switch_state = false; //Reset when entering a map with different name
        public static bool ScreenTransition = false; //Whether a screen transition is happening right now
        public static int ENTRANCE_GRID_X;
        public static int ENTRANCE_GRID_Y;
        public static int PLAYER_X;
        public static int PLAYER_Y;
        public static int CURRENT_GRID_X;
        public static int CURRENT_GRID_Y;
        public static int MAP_GRID_WIDTH;
        public static int MAP_GRID_HEIGHT;
        public static string CURRENT_MAP_NAME;
        public static int ENEMIES_KILLED;
        public static int PUZZLES_SOLVED;

        public delegate Touching MapChecker(Vector2 pos);
        public static MapChecker CheckTile;

        public static MinimapTracker minimaps = new MinimapTracker();

        public static Minimap CurrentMinimap => minimaps.GetMinimap(CURRENT_MAP_NAME);

        public static Facing NewMapFacing = Facing.RIGHT;

        public static bool WARP = false;
        public static string NEXT_MAP_NAME;
        public static DoorMapPair ReturnTarget = null;
        public static Vector2 PLAYER_WARP_TARGET;

        public static bool RefreshKeyCount = false;
        public static bool RefreshMaxHealth = false;
        public static bool RefreshLabels = false;

        public static bool[] ActivatedNexusPortals { get; private set; } = Enumerable.Repeat(false, 21).ToArray();

        public static bool autosave_on = true;
        //public static var checkpoint:Object = { x: 0, y: 0, area: "" };

        /**
         * Used for disabling the menu during an event because you could potentially break the game  otherwise
         */
        public static bool disable_menu = false;

        public static bool FUCK_IT_MODE_ON = false;

        //Health stuff
        public static int CUR_HEALTH = 6;
        public static int MAX_HEALTH = 6;

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

        public static Darkness darkness = new Darkness();
        public static ScreenShake screenShake = new();

        public static FadeEffect death_fadein = new FadeEffect() { fadeColor = Color.Black };
        public static FadeEffect black_overlay = new FadeEffect() { fadeColor = Color.Black };
        public static FlashEffect flash = new();

        public static Pixelate pixelation = new Pixelate();

        public static Wave wave = new Wave();

        public static List<IFullScreenEffect> gameEffects = new List<IFullScreenEffect>() { new Static(), darkness };
        public static List<IFullScreenEffect> fullScreenEffects = new List<IFullScreenEffect>() { death_fadein, black_overlay, new Glitch(), new GrayScale(), pixelation, wave, flash, screenShake };
        public static IEnumerable<IFullScreenEffect> AllEffects
        {
            get
            {
                return gameEffects.Concat(fullScreenEffects);
            }
        }


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
