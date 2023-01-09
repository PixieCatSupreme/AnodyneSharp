
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing.Effects;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Lights;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Logging;
using AnodyneSharp.MapData;
using AnodyneSharp.States;
using AnodyneSharp.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            public CheckPoint checkpoint = GlobalState.checkpoint;
            public CheckPoint ReturnTarget = GlobalState.ReturnTarget;
            public int PillarSwitchOn = GlobalState.PillarSwitchOn;

            public long playtime = PlayTime.Ticks;
            public int current_health = _curHealth;
            public int max_health = _maxHealth;
            public int deaths = DeathCount;

            public static Save GetSave(string path)
            {
                string save;
                try
                {
                    save = File.ReadAllText(path);
                }
                catch
                {
                    return null;
                }
                return FromString(save);
            }

            public void SaveTo(int id)
            {
                File.WriteAllText($"{GameConstants.SavePath}Saves/Save_{id + 1}.dat", ToString());
            }

            public override string ToString()
            {
                return JsonSerializer.Serialize<Save>(this, serializerOptions);
            }

            public static Save FromString(string s)
            {
                return JsonSerializer.Deserialize<Save>(s, serializerOptions);
            }
        }

        static public int CurrentSaveGame = 0;

        public static string serialized_quicksave = null;
        public static CheckPoint quicksave_checkpoint = null;

        public static void SaveGame(int? id = null)
        {
            new Save().SaveTo(id ?? CurrentSaveGame);
        }

        public static void LoadSave(Save s)
        {
            minimaps.interest = s.minimap_state;
            EntityManager.State = s.entity_state;
            DialogueManager.SceneTree = s.dialogue_state;
            DialogueManager.Reload();
            events = s.events;
            inventory = s.inventory;
            checkpoint = s.checkpoint;
            ReturnTarget = s.ReturnTarget;
            PillarSwitchOn = s.PillarSwitchOn;

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

            CURRENT_MAP_NAME = "";
            NEXT_MAP_NAME = "BLANK";
            PLAYER_WARP_TARGET = new Vector2(23, 130 - 20); //original reports 23,130 but ends up subtracting HEADER_HEIGHT(20) from Y value

            NewMapFacing = Facing.RIGHT;

            _maxHealth = 6;
            _curHealth = 6;

            DeathCount = 0;

            RefreshKeyCount = true;
            RefreshMaxHealth = true;

            AlwaysCellGraphics = false;
            GameMode = GameMode.Normal;

            InDeathRoom = false;

            minimaps = new();
            events = new();
            inventory = new();
            DialogueManager.SceneTree = null;
            DialogueManager.Reload();
            EntityManager.State = new();
            PauseState.Reset();

            disable_menu = false;
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
        public static bool LastDialogueFinished = true;
        public static bool SetDialogueMode = false;

        public static IEnumerator<CutsceneState.CutsceneEvent> StartCutscene = null;

        public static Point CurrentMapGrid
        {
            get
            {
                return new(CURRENT_GRID_X, CURRENT_GRID_Y);
            }
        }

        public static Point TopLeftTile => new(CURRENT_GRID_X * 10, CURRENT_GRID_Y * 10);

        public static Rectangle ScreenHitbox => new(CURRENT_GRID_X * 160, CURRENT_GRID_Y * 160, 160, 160);

        public static bool CanChangeBroom
        {
            get
            {
                return !InDeathRoom && !IsCell && !IsKnife;
            }
        }

        public static bool IsCell
        {
            get
            {
                return AlwaysCellGraphics || CURRENT_MAP_NAME == "CELL";
            }
        }

        public static bool IsKnife
        {
            get
            {
                return CURRENT_MAP_NAME == "SUBURB";
            }
        }

        public static bool IsDungeon
        {
            get
            {
                return ReturnTarget != null && ReturnTarget.map != "NEXUS";
            }
        }

        public static Settings settings = Settings.Load();
        public static bool ResolutionDirty = false;
        public static Language CurrentLanguage => settings.language;

        public static int PillarSwitchOn = 0; //Reset when entering a map with different name

        public static bool ScreenTransition = false; //Whether a screen transition is happening right now
        public static int CURRENT_GRID_X;
        public static int CURRENT_GRID_Y;
        public static int MAP_GRID_WIDTH;
        public static int MAP_GRID_HEIGHT;
        public static string CURRENT_MAP_NAME;
        public static int ENEMIES_KILLED;
        public static int PUZZLES_SOLVED;

        public static Action<Entity> SpawnEntity;

        public static Action<GameEvent> FireEvent;

        public static Action<State> SetSubstate;

        public static Action DoQuickSave;
        public static Action DoQuickLoad;

        public static IPublicMap Map;

        private static MinimapTracker minimaps = new();

        public static Minimap CurrentMinimap => minimaps.GetMinimap(CURRENT_MAP_NAME);

        public static EventRegister events = new();
        public static InventoryManager inventory = new();


        public static Facing? NewMapFacing = Facing.RIGHT;
        public static bool WARP = false;
        public static string NEXT_MAP_NAME;
        public static Vector2 PLAYER_WARP_TARGET;

        public static bool RefreshKeyCount = false;
        public static bool RefreshMaxHealth = false;
        public static bool RefreshLabels = false;

        public static bool BoiEaster = false;

        public record CheckPoint
        {
            public string map;
            public Vector2 Position;

            [JsonConstructor]
            public CheckPoint(string map, Vector2 position)
            {
                this.map = map;
                Position = position;
            }

            public CheckPoint(DoorMapPair doorpair)
            {
                map = doorpair.Map;
                Position = doorpair.Door.Position;
            }

            public void Warp(Vector2 offset)
            {
                NEXT_MAP_NAME = map;
                PLAYER_WARP_TARGET = Position + offset;
                WARP = true;
            }
        }
        public static CheckPoint checkpoint;
        public static CheckPoint ReturnTarget;

        /**
         * Used for disabling the menu during an event because you could potentially break the game  otherwise
         */
        public static bool disable_menu = false;

        public static bool FUCK_IT_MODE_ON = false;

        public static bool draw_hitboxes = false;

        //Health stuff
        public static int CUR_HEALTH
        {
            get
            {
                return _curHealth;
            }
            set
            {
                _curHealth = Math.Clamp(value,0,MAX_HEALTH);
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
        public static bool ToTitle = false;

        public static Light PlayerLight = null;

        public static Darkness darkness = new Darkness();
        public static FadeEffect gameScreenFade = new() { fadeColor = Color.Black };

        public static TitleScreenOverlay TitleScreenFinish = new TitleScreenOverlay();

        public static Static staticEffect = new();

        public static ScreenShake screenShake = new();

        public static FadeEffect black_overlay = new FadeEffect() { fadeColor = Color.Black };
        public static FlashEffect flash = new();

        public static Pixelate pixelation = new Pixelate();

        public static FG_Blend fgBlend = new();
        public static Blend extraBlend = new();


        public static Wave wave = new();
        public static Glitch glitch = new();

        public static List<IFullScreenEffect> gameEffects = new List<IFullScreenEffect>() { fgBlend, staticEffect, darkness, gameScreenFade };
        public static List<IFullScreenEffect> fullScreenEffects = new List<IFullScreenEffect>() { black_overlay, glitch, new GrayScale(), TitleScreenFinish, pixelation, extraBlend, wave, flash, screenShake };
        public static IEnumerable<IFullScreenEffect> AllEffects
        {
            get
            {
                return gameEffects.Concat(fullScreenEffects);
            }
        }

        public static int DeathCount = 0;


        //Effect deciding bool
        public static bool InDeathRoom;

        private static string _dialogue;

        public static List<UIEntity> UIEntities = new List<UIEntity>();
    }
}
