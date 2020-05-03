using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Input;
using AnodyneSharp.Map;
using AnodyneSharp.Map.Tiles;
using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.UI.Font;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using static AnodyneSharp.Registry.GameConstants;

namespace AnodyneSharp.States
{
    public enum PlayStateState
    {
        S_NORMAL,
        S_TRANSITION,
        S_PAUSED,
        S_PLAYER_DIED = 4,
        S_JUST_ENTERED_MAP,
        S_DIRECT_CONTROLS,
        S_CUTSCENE,
        S_DIALOGUE
    }

    public class PlayState : State
    {
        public const string UiHeader = "header";

        public const int Scroll_Increment = 4;

        private PlayStateState _state;

        private TileMap _map;
        private TileMap _map_bg_2; //on top of the bg map
        private TileMap _map_fg; // on top of all sprites but below darkness

        private Player _player;

        private Camera _camera;
        private bool _justTransitioned;

        private Rectangle _gridBorders;

        private HealthBar _healthBar;
        private Texture2D _header;

        private CollisionGroups _groups; //Deals with entity-map and entity-entity collision

        private List<Entity> _gridEntities; //Holds entities that stay on the current grid coordinate
        private List<Entity> _oldEntities; //Holds entities that will despawn after a screen transition is complete

        private State _childState;

        private bool _updateEntities;
        private UILabel _keyValueLabel;

        private Texture2D _equippedBroomBorder;
        private Texture2D _equippedBroomIcon;

        private Vector2 _iconPos;

        public PlayState(Camera camera)
        {
            _map = new TileMap();
            _map_bg_2 = new TileMap();
            _map_fg = new TileMap();

            _gridEntities = new List<Entity>();
            _oldEntities = new List<Entity>();

            _camera = camera;

            _player = new Player(this);
            _healthBar = new HealthBar(new Vector2(155, 2));

            _updateEntities = true;

            _keyValueLabel = new UILabel(new Vector2(37, 5), false);
            _keyValueLabel.Writer.SetSpriteFont(FontManager.InitFont(new Color(124, 163, 177, 255)));
            _keyValueLabel.SetText("x0");

            _iconPos = new Vector2(2, 3);
        }

        public override void Create()
        {
            base.Create();

            _header = ResourceManager.GetTexture(UiHeader);

            _equippedBroomBorder = ResourceManager.GetTexture("frame_icon");

            LoadMap();

            UpdateBroomIcon();
        }

        public override void Draw()
        {
#if DEBUG
            if (GlobalState.DrawBG)
            {
                _map.Draw();
            }
            if (GlobalState.DrawBG2)
            {
                _map_bg_2.Draw(true);
            }
            if (GlobalState.DrawFG)
            {
                _map_fg.Draw(true);
            }

#else
            _map.Draw();
            _map_bg_2.Draw(0.1f, true);
            _map_fg.Draw(0.2f, true);
#endif

            _player.Draw();

            foreach (Entity gridEntity in _gridEntities)
            {
                gridEntity.Draw();
            }
            foreach (Entity gridEntity in _oldEntities)
            {
                gridEntity.Draw();
            }

            if (_childState != null)
            {
                _childState.Draw();
            }
        }

        public override void DrawUI()
        {
            SpriteDrawer.DrawGuiSprite(_header, Vector2.Zero, Z: DrawingUtilities.GetDrawingZ(DrawOrder.HEADER));


            if (InventoryState.EquippedBroom != BroomType.NONE)
            {
                SpriteDrawer.DrawGuiSprite(_equippedBroomIcon, _iconPos, scale: 0.80f, Z: DrawingUtilities.GetDrawingZ(DrawOrder.UI_OBJECTS));
            }

            SpriteDrawer.DrawGuiSprite(_equippedBroomBorder, _iconPos, scale: 0.80f, Z: DrawingUtilities.GetDrawingZ(DrawOrder.EQUIPPED_BORDER));


            _healthBar.Draw();

            if (_childState != null)
            {
                _childState.DrawUI();
            }


            _keyValueLabel.Draw();
        }

        public override void Update()
        {
            switch (_state)
            {
                case PlayStateState.S_NORMAL:
                    StateNormal();
                    break;
                case PlayStateState.S_TRANSITION:
                    //Registry.sound_data.current_song.volume = FlxG.volume * Registry.volume_scale;
                    StateTransition();
                    DoCollisions();
                    UpdateEntities();
                    return;
                case PlayStateState.S_PAUSED:
                    if (_childState is PauseState pauseState)
                    {
                        _player.dontMove = true;
                        pauseState.Update();

                        if (pauseState.Exited)
                        {
                            _state = PlayStateState.S_NORMAL;
                            _childState = null;
                            _player.dontMove = false;
                            _player.broom.UpdateBroomType();
                            _updateEntities = true;
                        }
                    }
                    break;
                case PlayStateState.S_PLAYER_DIED:
                    break;
                case PlayStateState.S_JUST_ENTERED_MAP:
                    break;
                case PlayStateState.S_DIRECT_CONTROLS:
                    break;
                case PlayStateState.S_CUTSCENE:
                    break;
                case PlayStateState.S_DIALOGUE:
                    if (_childState is DialogueState dialogueState)
                    {
                        _player.dontMove = true;
                        dialogueState.Update();

                        if (GlobalState.cur_dialogue == "")
                        {
                            _state = PlayStateState.S_NORMAL;
                            _childState = null;
                            _player.dontMove = false;
                        }
                    }
                    break;
                default:
                    break;
            }

            if (_updateEntities)
            {
                if (!_justTransitioned)
                {
                    DoCollisions();
                }

                UpdateEntities();
            }

            if (InventoryState.EquippedBroomChanged)
            {
                InventoryState.EquippedBroomChanged = false;

                UpdateBroomIcon();
            }

#if DEBUG
            DebugKeyInput();
#endif
        }

        public Touching GetTileCollisionFlags(Vector2 position)
        {
            return _map.GetCollisionData(position) | _map_bg_2.GetCollisionData(position);
        }

        private void UpdateEntities()
        {
            _player.Update();
            _player.PostUpdate();

            foreach (Entity gridEntity in _gridEntities.Where(e=>e.exists))
            {
                gridEntity.Update();
                gridEntity.PostUpdate();
            }

            _healthBar.Update();
            UpdateHealth();
        }

        private void DoCollisions()
        {
            _groups.DoCollision(_map, _map_bg_2);
        }

        private void StateNormal()
        {
            CheckForTransition();

            _keyValueLabel.SetText($"x{InventoryState.GetCurrentMapKeys()}");

            if (KeyInput.CanPressKey(Keys.Enter))
            {
                _childState = new PauseState();
                _state = PlayStateState.S_PAUSED;
                _updateEntities = false;
            }

            //TODO check if player is unalive
        }

        private void CheckForTransition()
        {
            _state = PlayStateState.S_TRANSITION;
            if (_player.Position.X < _gridBorders.X)
            {
                GlobalState.CURRENT_GRID_X--;
                _player.Position.X = _gridBorders.X - _player.width;
            }
            else if (_player.Position.Y < _gridBorders.Y)
            {
                GlobalState.CURRENT_GRID_Y--;

                _player.Position.Y = _gridBorders.Y - _player.height;
            }
            else if (_player.Position.Y > _gridBorders.Bottom - _player.height)
            {
                GlobalState.CURRENT_GRID_Y++;
                _player.Position.Y = _gridBorders.Y + _gridBorders.Height;
            }
            else if (_player.Position.X > _gridBorders.Right - _player.width)
            {
                GlobalState.CURRENT_GRID_X++;
                _player.Position.X = _gridBorders.Right;
            }
            else
            {
                _state = PlayStateState.S_NORMAL;
            }

            //debugText.text += " ub: " + upperBorder.toString() + "leb: " + leftBorder.toString() + "\n camx: " + FlxG.camera.bounds.x.toString() + "camy: " + FlxG.camera.bounds.y.toString() + 
            //"\n x: " + player.x.toFixed(2) + " y: " + player.y.toFixed(2);

            if (_state == PlayStateState.S_TRANSITION)
            {
                _player.grid_entrance_x = _player.Position.X;
                _player.grid_entrance_y = _player.Position.Y;
                _justTransitioned = true;
                _player.dontMove = true;

                //TODO maybe put this in Player.cs to get the transition bug
                _player.velocity = Vector2.Zero;
            }
        }

        private void StateTransition()
        {
            _player.invincible = true;

            if (_justTransitioned)
            {
                //TODO add enemy, puzzle and tile resetting on grid change
                UpdateScreenBorders();
                LoadGridEntities();

                _justTransitioned = false;
            }

            if (!UpdateCamera())
            {
                _player.invincible = false;

                //delete old objects
                _oldEntities.Clear();

                // TODO update miniminimap

                //TODO reset broom

                _player.dontMove = false;

                //TODO update minimap

                _state = PlayStateState.S_NORMAL;
            }
        }

        private void UpdateScreenBorders()
        {
            _gridBorders.X = GlobalState.CURRENT_GRID_X * SCREEN_WIDTH_IN_PIXELS;
            _gridBorders.Width = SCREEN_WIDTH_IN_PIXELS;
            _gridBorders.Y = GlobalState.CURRENT_GRID_Y * SCREEN_HEIGHT_IN_PIXELS;
            _gridBorders.Height = SCREEN_HEIGHT_IN_PIXELS;
        }

        private bool UpdateCamera()
        {
            if (_camera.Position2D.X < _gridBorders.X - Scroll_Increment)
            {
                _camera.Move(Scroll_Increment, 0);
                return true;
            }
            else if (_camera.Position2D.Y < _gridBorders.Y - Scroll_Increment - HEADER_HEIGHT)
            {
                _camera.Move(0, Scroll_Increment);
                return true;
            }
            else if (_camera.Position2D.Y > _gridBorders.Y + Scroll_Increment - HEADER_HEIGHT)
            {
                _camera.Move(0, -Scroll_Increment);
                return true;
            }
            else if (_camera.Position2D.X > _gridBorders.Left + Scroll_Increment)
            {
                _camera.Move(-Scroll_Increment, 0);
                return true;
            }
            else
            {
                _camera.GoTo(_gridBorders.X, _gridBorders.Y);
                return false;
            }
        }

#if DEBUG
        private void DebugKeyInput()
        {
            if (KeyInput.CanPressKey(Keys.T))
            {
                GlobalState.cur_dialogue = "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890.:,;'\"(!?)+-*/=[]";
                GlobalState.cur_dialogue = "Hello^\nYes, this is slime";

                _state = PlayStateState.S_DIALOGUE;
                _childState = new DialogueState();
            }

            if (KeyInput.CanPressKey(Keys.F5))
            {
                _keyValueLabel.SetText($"x{InventoryState.RemoveCurrentMapKey()}");
            }
            else
            if (KeyInput.CanPressKey(Keys.F6))
            {
                _keyValueLabel.SetText($"x{InventoryState.AddCurrentMapKey()}");
            }

            if (KeyInput.CanPressKey(Keys.M))
            {
                GlobalState.CURRENT_MAP_NAME = TileData.GetNextMapName();
                LoadMap();
                StateTransition();
            }
            else if (KeyInput.CanPressKey(Keys.N))
            {
                GlobalState.CURRENT_MAP_NAME = TileData.GetPreviousMapName();
                LoadMap();
                StateTransition();
            }

            if (KeyInput.CanPressKey(Keys.D1))
            {
                InventoryState.EquippedBroom = BroomType.Normal;
                _player.broom.UpdateBroomType();
            }
            else if (KeyInput.CanPressKey(Keys.D2))
            {
                InventoryState.EquippedBroom = BroomType.Wide;
                _player.broom.UpdateBroomType();
            }
            else if (KeyInput.CanPressKey(Keys.D3))
            {
                InventoryState.EquippedBroom = BroomType.Long;
                _player.broom.UpdateBroomType();
            }
            else if (KeyInput.CanPressKey(Keys.D4))
            {
                InventoryState.EquippedBroom = BroomType.NONE;
                _player.broom.UpdateBroomType();
            }

            if (KeyInput.CanPressKey(Keys.Space))
            {
                GlobalState.CUR_HEALTH -= 3;
            }
            else if (KeyInput.CanPressKey(Keys.Enter))
            {
                GlobalState.CUR_HEALTH += 3;
            }

            if (KeyInput.CanPressKey(Keys.F1))
            {
                GlobalState.DrawBG = !GlobalState.DrawBG;
            }
            if (KeyInput.CanPressKey(Keys.F2))
            {
                GlobalState.DrawBG2 = !GlobalState.DrawBG2;
            }
            if (KeyInput.CanPressKey(Keys.F3))
            {
                GlobalState.DrawFG = !GlobalState.DrawFG;
            }

            if (KeyInput.CanPressKey(Keys.OemPlus))
            {
                _camera.Zoom += 0.1f;
            }
            else if (KeyInput.CanPressKey(Keys.OemMinus) && _camera.Zoom > 0)
            {
                _camera.Zoom -= 0.1f;
            }

            if (GlobalState.MovingCamera)
            {
                float camSpeed = !GlobalState.FreeRoamCamera ? SCREEN_WIDTH_IN_PIXELS : KeyInput.IsKeyPressed(Keys.LeftShift) ? 8 : 4;

                if (KeyInput.CanPressKey(Keys.NumPad4))
                {
                    _camera.Move(-camSpeed, 0);
                }
                else if (KeyInput.CanPressKey(Keys.NumPad6))
                {
                    _camera.Move(camSpeed, 0);
                }

                if (KeyInput.CanPressKey(Keys.NumPad8))
                {
                    _camera.Move(0, -camSpeed);
                }
                else if (KeyInput.CanPressKey(Keys.NumPad2))
                {
                    _camera.Move(0, camSpeed);
                }

                if (KeyInput.CanPressKey(Keys.D1))
                {
                    _camera.GoTo(MapUtilities.GetRoomUpperLeftPos(new Vector2(1, 5)));
                }
            }
        }
#endif

        private void UpdateHealth()
        {
            var result = _healthBar.UpdateHealth();

            if (result == false)
            {
                //DIE
            }
        }

        private void UpdateBroomIcon()
        {
            if (InventoryState.EquippedBroom == BroomType.NONE)
            {
                return;
            }

            string tex = "";

            switch (InventoryState.EquippedBroom)
            {
                case BroomType.Normal:
                    tex = "none";
                    break;
                case BroomType.Wide:
                    tex = "wide";
                    break;
                case BroomType.Long:
                    tex = "long";
                    break;
                case BroomType.Transformer:
                    tex = "transformer";
                    break;
            }

            _equippedBroomIcon = ResourceManager.GetTexture(tex + "_icon");
        }


        private void LoadMap()
        {
            TileData.SetTileset(GlobalState.CURRENT_MAP_NAME);
            _map.LoadMap(MapLoader.GetMap(GlobalState.CURRENT_MAP_NAME), TileData.Tiles, DrawOrder.MAP_BG);

            _map_bg_2.LoadMap(MapLoader.GetMap(GlobalState.CURRENT_MAP_NAME, 2), TileData.Tiles, DrawOrder.MAP_BG2);
            _map_bg_2.y = HEADER_HEIGHT;
            _map_fg.LoadMap(MapLoader.GetMap(GlobalState.CURRENT_MAP_NAME, 3), TileData.Tiles, DrawOrder.MAP_FG);
            _map_fg.y = HEADER_HEIGHT;

            //Sets tile collission and tile events
            TileData.Set_tile_properties(_map, _map_bg_2);
            _player.Position = _map.GetFirstWalkable(_map_bg_2) * TILE_WIDTH;

            Vector2 gridPos = MapUtilities.GetRoomCoordinate(_player.Position);
            Vector2 roomPos = MapUtilities.GetRoomUpperLeftPos(gridPos);

            GlobalState.CURRENT_GRID_X = (int)gridPos.X;
            GlobalState.CURRENT_GRID_Y = (int)gridPos.Y;

            _player.Reset();

            _camera.GoTo(roomPos);

            UpdateScreenBorders();

            foreach(EntityPreset p in EntityManager.GetMapEntities(GlobalState.CURRENT_MAP_NAME).Where(p=>p.Permanence == Permanence.MAP_LOCAL))
            {
                p.Alive = true;
            }

            LoadGridEntities();

            _keyValueLabel.SetText($"x{InventoryState.GetCurrentMapKeys()}");

            PlayMapMusic();
        }

        private void PlayMapMusic()
        {
            string title = GlobalState.CURRENT_MAP_NAME.ToLower();
            if (GlobalState.OnRoof)
            {
                //TODO maybe BOI or a different easter egg for redcave?
                title = "roof";
            }
            else if (!GlobalState.HappyStarted && title == "happy")
            {
                title = "happy-init";
            }
            else if (!GlobalState.SageDead && title == "terminal")
            {
                title = "pre_terminal";
            }
            else if (GlobalState.SuburbSoft && title == "suburb")
            {
                title = "soft";
            }
            else if (title == "drawer")
            {
                title = "suburb";
            }
            else if ((!GlobalState.WindmillOpened && title == "windmill") ||GlobalState.InDeathRoom || title == "debug")
            {
                SoundManager.StopSong();
                return;
            }

            SoundManager.PlaySong(title);
        }

        private void LoadGridEntities()
        {
            _groups = new CollisionGroups();
            _groups.Register(_player);
            _groups.Register(_player.broom);

            _oldEntities = new List<Entity>(_gridEntities);

            List<EntityPreset> gridPresets = EntityManager.GetGridEntities(GlobalState.CURRENT_MAP_NAME, new Vector2(GlobalState.CURRENT_GRID_X, GlobalState.CURRENT_GRID_Y));
            foreach (EntityPreset preset in gridPresets.Where(e=>e.Permanence == Permanence.GRID_LOCAL))
            {
                preset.Alive = true;
            }

            _gridEntities = gridPresets.Where(preset => preset.Alive)
                .Select(preset => preset.Create()).SelectMany(e => new List<Entity> { e }.Concat(e.SubEntities())).ToList();
            foreach (Entity e in _gridEntities)
            {
                _groups.Register(e);
            }
        }
    }
}
