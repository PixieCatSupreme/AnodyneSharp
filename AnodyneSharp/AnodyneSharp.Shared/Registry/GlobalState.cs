
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing.Effects;
using AnodyneSharp.Entities;
using AnodyneSharp.Logging;
using AnodyneSharp.Map;
using AnodyneSharp.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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
        public class Save
        {
            public static JsonSerializerOptions serializerOptions = new()
            {
                IncludeFields = true
            };

            public Dictionary<string, List<int>> minimap_state = GlobalState.minimaps.interest;
            public Dictionary<Guid, EntityState> entity_state = EntityManager.State;
            public Dictionary<string, DialogueNPC> dialogue_state = DialogueManager.SceneTree;
            public EventRegister events = GlobalState.events;
            public InventoryManager inventory = GlobalState.inventory;
            public AchievementManager achievements = GlobalState.achievements;
            public CheckPoint checkpoint = GlobalState.checkpoint;
            public long playtime = PlayTime.Ticks;
            public int current_health = _curHealth;
            public int max_health = _maxHealth;
            public int deaths = DeathCount;

            public static Save getSave(string path)
            {
                try
                {
                    string save = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<Save>(save, serializerOptions);
                }
                catch
                {
                    return null;
                }
            }
        }

        static public int CurrentSaveGame = 0;

        public static void SaveGame()
        {
            File.WriteAllText($"Save_{CurrentSaveGame + 1}.dat", JsonSerializer.Serialize<Save>(new Save(), Save.serializerOptions));
        }

        public static void LoadSave(Save s)
        {
            minimaps.interest = s.minimap_state;
            EntityManager.State = s.entity_state;
            DialogueManager.SceneTree = s.dialogue_state;
            DialogueManager.LoadDialogue(settings.language);
            events = s.events;
            inventory = s.inventory;
            achievements = s.achievements;
            checkpoint = s.checkpoint;

            PLAYER_WARP_TARGET = checkpoint.Position;
            NEXT_MAP_NAME = checkpoint.map;
            
            _totalPreviously = new TimeSpan(s.playtime);
            MAX_HEALTH = s.max_health;
            CUR_HEALTH = s.current_health;
            DeathCount = s.deaths;
        }

        public static void ResetValues()
        {
            START_TIME = DateTime.Now;

            FishermanDead = false;

            CURRENT_MAP_NAME = "";
            NEXT_MAP_NAME = "BLANK";
            PLAYER_WARP_TARGET = new Vector2(23, 130 - 20); //original reports 23,130 but ends up subtracting HEADER_HEIGHT(20) from Y value

            NewMapFacing = Facing.RIGHT;

            _maxHealth = 6;
            _curHealth = 6;

            RefreshKeyCount = true;
            RefreshMaxHealth = true;

            AlwaysCellGraphics = false;
            GameMode = GameMode.Normal;

            OnRoof = false;
            InDeathRoom = false;
            HappyStarted = false;
            SageDead = false;
            SuburbSoft = false;
            WindmillOpened = false;

            minimaps = new();
            events = new();
            inventory = new();
            achievements = new();
        }

        private static TimeSpan _totalPreviously;
        private static DateTime START_TIME;

        public static TimeSpan PlayTime => _totalPreviously + (DateTime.Now - START_TIME);

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
                LastDialogueFinished = value == "";
                if (value == "") DialogueTop = false;
            }
        }

        public static bool DialogueTop = false;
        public static bool LastDialogueFinished = false;
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
                return !InDeathRoom && !IsCell && CURRENT_MAP_NAME != "SUBURB";
            }
        }

        public static bool IsCell
        {
            get
            {
                return AlwaysCellGraphics || CURRENT_MAP_NAME == "CELL";
            }
        }

        public static bool IsDungeon
        {
            get
            {
                return ReturnTarget != null && ReturnTarget.Map != "NEXUS";
            }
        }

        public static bool FishermanDead { get; set; }

        public static Settings settings = Settings.Load();
        public static Language CurrentLanguage => settings.language;

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

        public delegate void Spawn(Entity e);
        public static Spawn SpawnEntity;

        private static MinimapTracker minimaps = new();

        public static Minimap CurrentMinimap => minimaps.GetMinimap(CURRENT_MAP_NAME);

        public static EventRegister events = new();
        public static InventoryManager inventory = new();
        public static AchievementManager achievements = new();


        public static Facing NewMapFacing = Facing.RIGHT;
        public static bool WARP = false;
        public static string NEXT_MAP_NAME;
        public static DoorMapPair ReturnTarget = null;
        public static Vector2 PLAYER_WARP_TARGET;

        public static bool RefreshKeyCount = false;
        public static bool RefreshMaxHealth = false;
        public static bool RefreshLabels = false;

        public record CheckPoint
        {
            public string map;
            public Vector2 Position;

            public CheckPoint(string map, Vector2 position)
            {
                this.map = map;
                Position = position;
            }
        }
        public static CheckPoint checkpoint;

        /**
         * Used for disabling the menu during an event because you could potentially break the game  otherwise
         */
        public static bool disable_menu = false;

        public static bool FUCK_IT_MODE_ON = false;

        //Health stuff
        public static int CUR_HEALTH
        {
            get
            {
                return _curHealth;
            }
            set
            {
                _curHealth = value > 0 ? value : 0;
            }
        }

        public static int MAX_HEALTH
        {
            get
            {
                return _maxHealth;
            }
            set
            {
                _maxHealth = value <= 16 ? value : 16;
                RefreshMaxHealth = true;
            }
        }

        private static int _curHealth = 6;
        private static int _maxHealth = 6;

        //Cheatz stuff
        public static bool AlwaysCellGraphics = false;
        public static GameMode GameMode = GameMode.Normal;
        public static bool ForceTextureReload = false;

        public static Random RNG = new Random();

        public static bool ShowFPS = false;

        //Cool new debug values
#if DEBUG
        public static bool DrawBG = true;
        public static bool DrawBG2 = true;
        public static bool DrawFG = true;

        public static bool MovingCamera = true;
        public static bool FreeRoamCamera = false;
#endif

        public static bool ClosingGame = false;

        public static Darkness darkness = new Darkness();
        public static TitleScreenOverlay TitleScreenFinish = new TitleScreenOverlay();

        public static ScreenShake screenShake = new();

        public static FadeEffect black_overlay = new FadeEffect() { fadeColor = Color.Black };
        public static FlashEffect flash = new();

        public static Pixelate pixelation = new Pixelate();

        public static Wave wave = new Wave();

        public static List<IFullScreenEffect> gameEffects = new List<IFullScreenEffect>() { new Static(), darkness };
        public static List<IFullScreenEffect> fullScreenEffects = new List<IFullScreenEffect>() { black_overlay, new Glitch(), new GrayScale(), TitleScreenFinish, pixelation, wave, flash, screenShake };
        public static IEnumerable<IFullScreenEffect> AllEffects
        {
            get
            {
                return gameEffects.Concat(fullScreenEffects);
            }
        }

        public static int DeathCount = 0;


        //Song deciding bools
        public static bool OnRoof;
        public static bool InDeathRoom;
        public static bool HappyStarted;
        public static bool SageDead;
        public static bool SuburbSoft;
        public static bool WindmillOpened;

        private static string _dialogue;

        public static List<UIEntity> UIEntities = new List<UIEntity>();
    }
}
