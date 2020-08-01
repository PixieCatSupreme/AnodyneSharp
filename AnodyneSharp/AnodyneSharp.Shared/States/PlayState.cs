using AnodyneSharp.Cheatz;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Enemy;
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
using System;
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

        private float _texRandomTimer;

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

            _iconPos = new Vector2(2, 3);

            SetKeyLabel();
        }

        public override void Create()
        {
            base.Create();

            _header = ResourceManager.GetTexture(UiHeader, true);

            _equippedBroomBorder = ResourceManager.GetTexture("frame_icon", true);

            LoadMap();
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
            _map_bg_2.Draw(true);
            _map_fg.Draw(true);
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


            if (InventoryManager.EquippedBroom != BroomType.NONE && _equippedBroomIcon != null)
            {
                SpriteDrawer.DrawGuiSprite(_equippedBroomIcon, _iconPos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.UI_OBJECTS));
            }

            SpriteDrawer.DrawGuiSprite(_equippedBroomBorder, _iconPos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.EQUIPPED_BORDER));


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
                        pauseState.Update();

                        if (pauseState.Exited)
                        {
                            _state = PlayStateState.S_NORMAL;
                            _childState = null;
                            _player.broom.UpdateBroomType();
                            _updateEntities = true;
                            return;
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
                    _player.dontMove = true;
                    _player.skipBroom = true;
                    _childState.Update();

                    if (GlobalState.Dialogue == "")
                    {
                        _state = PlayStateState.S_NORMAL;
                        _childState = null;
                        _player.dontMove = false;
                        _player.skipBroom = false;
                    }
                    break;
                default:
                    break;
            }

            if (_updateEntities)
            {
                if (_state != PlayStateState.S_TRANSITION)
                {
                    DoCollisions();
                }

                UpdateEntities();
            }

            if (InventoryManager.EquippedBroomChanged)
            {
                InventoryManager.EquippedBroomChanged = false;

                UpdateBroomIcon();
            }

            if (GlobalState.RefreshLabels)
            {
                SetKeyLabel();
            }

#if DEBUG
            DebugKeyInput();
#endif

            Refreshes();
        }

        public Touching GetTileCollisionFlags(Vector2 position)
        {
            return _map.GetCollisionData(position) | _map_bg_2.GetCollisionData(position);
        }

        private void Refreshes()
        {
            if (GlobalState.RefreshMaxHealth)
            {
                GlobalState.RefreshMaxHealth = false;
                _healthBar.CreateHealthBoxes();
            }

            UpdateHealth();
            _healthBar.Update();

            if (GlobalState.RefreshKeyCount)
            {
                GlobalState.RefreshKeyCount = false;
                _keyValueLabel.SetText($"x{InventoryManager.GetCurrentMapKeys()}");
            }

            if (GlobalState.GameMode == GameMode.EXTREME_CHAOS)
            {
                _texRandomTimer += GameTimes.DeltaTime;

                if (_texRandomTimer >= 0.8f)
                {
                    GlobalState.ForceTextureReload = true;
                    _texRandomTimer = 0f;
                }
            }

            if (GlobalState.ForceTextureReload)
            {
                GlobalState.ForceTextureReload = false;

                _player.Reset();
                _player.ReloadTexture();
                ReloadMapTextures();

                foreach (var item in _gridEntities)
                {
                    item.ReloadTexture();
                }

            }

            GlobalState.ENEMIES_KILLED = _groups.KilledEnemies();
        }

        private void UpdateEntities()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) && CheckInteraction())
            {
                _player.skipBroom = true;
            }
            _player.Update();
            _player.PostUpdate();

            foreach (Entity gridEntity in _gridEntities.Where(e => e.exists))
            {
                gridEntity.Update();
                gridEntity.PostUpdate();
            }
        }

        private bool CheckInteraction()
        {
            if (_player.state != PlayerState.GROUND || _state == PlayStateState.S_DIALOGUE)
            {
                return false;
            }

            Rectangle InteractHitbox(Entity e)
            {
                Rectangle ret = e.Hitbox;
                Vector2 facing_vec = -Entity.FacingDirection(_player.facing);
                //Increase size in the correct dimension by 2 pixels
                ret.Inflate(Math.Abs(facing_vec.X), Math.Abs(facing_vec.Y));
                //Move rectangle to have the two pixel buffer on the correct side.
                ret.Offset(facing_vec.X, facing_vec.Y);
                return ret;
            }

            return _gridEntities.OfType<Interactable>().Any(i => (InteractHitbox((Entity)i).Intersects(_player.Hitbox) && i.PlayerInteraction(_player.facing)));
        }

        private void DoCollisions()
        {
            _groups.DoCollision(_map, _map_bg_2);
        }

        private void StateNormal()
        {
            if (GlobalState.SetDialogueMode)
            {
                _state = PlayStateState.S_DIALOGUE;
                _childState = new DialogueState();
                _player.BeIdle();
                return;
            }

            CheckForTransition();

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Pause))
            {
                _childState = new PauseState();
                _state = PlayStateState.S_PAUSED;
                _updateEntities = false;
                SoundManager.PlaySoundEffect("pause_sound");
                return;
            }

            if (!_player.broom.exists)
            {
                if (KeyInput.JustPressedRebindableKey(KeyFunctions.NextPage))
                {
                    SwitchBroom(true);
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.PreviousPage))
                {
                    SwitchBroom(false);
                }

                if (KeyInput.JustPressedKey(Keys.D1))
                {
                    SetBroom(BroomType.Normal);
                }
                else if (KeyInput.JustPressedKey(Keys.D3))
                {
                    SetBroom(BroomType.Wide);
                }
                else if (KeyInput.JustPressedKey(Keys.D2))
                {
                    SetBroom(BroomType.Long);
                }
                else if (KeyInput.JustPressedKey(Keys.D4))
                {
                    SetBroom(BroomType.Transformer);
                }
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
                _player.grid_entrance = _player.Position;
                _player.dontMove = true;

                _player.velocity = Vector2.Zero;

                UpdateScreenBorders();
                LoadGridEntities();
            }
        }

        private void StateTransition()
        {
            _player.invincible = true;

            if (!UpdateCamera())
            {
                _player.invincible = false;

                //delete old objects
                _oldEntities.Clear();

                // TODO update miniminimap

                _player.dontMove = false;

                //TODO update minimap

                _state = PlayStateState.S_NORMAL;
            }
        }

        private void SetKeyLabel()
        {
            _keyValueLabel = new UILabel(new Vector2(37, 5 - LineOffset + (GlobalState.CurrentLanguage == Language.ZH_CN ? 1 : 0)), false);
            _keyValueLabel.Writer.SetSpriteFont(FontManager.InitFont(new Color(124, 163, 177, 255)));
            _keyValueLabel.SetText($"x{InventoryManager.GetCurrentMapKeys()}");
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
            if (GlobalState.FUCK_IT_MODE_ON)
            {
                _camera.GoTo(_gridBorders.X, _gridBorders.Y);
                return false;
            }

            if (_camera.Position2D.X < _gridBorders.X - Scroll_Increment)
            {
                _camera.Move(Scroll_Increment, 0);
                return true;
            }
            else if (_camera.Position2D.Y < _gridBorders.Y - Scroll_Increment)
            {
                _camera.Move(0, Scroll_Increment);
                return true;
            }
            else if (_camera.Position2D.Y > _gridBorders.Y + Scroll_Increment)
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
            if (KeyInput.JustPressedKey(Keys.T))
            {
                GlobalState.Dialogue = FontManager.LanguageString;
                //GlobalState.Dialogue = "Hello^\nYes, this is slime";
            }

            if (KeyInput.JustPressedKey(Keys.F6))
            {
                Cheatz.Cheatz.GiveKey();
            }

            if (KeyInput.JustPressedKey(Keys.F11))
            {
                Cheatz.Cheatz.ToggleFuckItMode();
            }

            if (KeyInput.JustPressedKey(Keys.F10))
            {
                Cheatz.Cheatz.KonamiCode();
            }
            if (KeyInput.JustPressedKey(Keys.F9))
            {
                Cheatz.Cheatz.UnlockNexusGates();
            }

            if (KeyInput.JustPressedKey(Keys.M))
            {
                GlobalState.CURRENT_MAP_NAME = TileData.GetNextMapName();
                LoadMap();
                StateTransition();
            }
            else if (KeyInput.JustPressedKey(Keys.N))
            {
                GlobalState.CURRENT_MAP_NAME = TileData.GetPreviousMapName();
                LoadMap();
                StateTransition();
            }

            if (KeyInput.JustPressedKey(Keys.D5))
            {
                SetBroom(BroomType.NONE);
            }

            if (KeyInput.JustPressedKey(Keys.F1))
            {
                GlobalState.DrawBG = !GlobalState.DrawBG;
            }
            if (KeyInput.JustPressedKey(Keys.F2))
            {
                GlobalState.DrawBG2 = !GlobalState.DrawBG2;
            }
            if (KeyInput.JustPressedKey(Keys.F3))
            {
                GlobalState.DrawFG = !GlobalState.DrawFG;
            }

            if (KeyInput.JustPressedKey(Keys.OemPlus))
            {
                _camera.Zoom += 0.1f;
            }
            else if (KeyInput.JustPressedKey(Keys.OemMinus) && _camera.Zoom > 0)
            {
                _camera.Zoom -= 0.1f;
            }

            if (GlobalState.MovingCamera)
            {
                float camSpeed = !GlobalState.FreeRoamCamera ? SCREEN_WIDTH_IN_PIXELS : KeyInput.IsKeyPressed(Keys.LeftShift) ? 8 : 4;

                if (KeyInput.JustPressedKey(Keys.NumPad4))
                {
                    _camera.Move(-camSpeed, 0);
                }
                else if (KeyInput.JustPressedKey(Keys.NumPad6))
                {
                    _camera.Move(camSpeed, 0);
                }

                if (KeyInput.JustPressedKey(Keys.NumPad8))
                {
                    _camera.Move(0, -camSpeed);
                }
                else if (KeyInput.JustPressedKey(Keys.NumPad2))
                {
                    _camera.Move(0, camSpeed);
                }
            }
        }
#endif
        private void SwitchBroom(bool nextBroom)
        {
            if (!InventoryManager.HasBroom)
            {
                return;
            }

            BroomType broomType = InventoryManager.EquippedBroom;

            if (!GlobalState.CanChangeBroom)
            {
                broomType = broomType == BroomType.Transformer ? BroomType.Normal : BroomType.Transformer;
            }
            else
            {
                do
                {
                    broomType += nextBroom ? 1 : -1;
                    broomType = (BroomType)(((int)broomType + (int)BroomType.Transformer + 1) % ((int)BroomType.Transformer + 1));
                } while (!InventoryManager.HasBroomType(broomType));
            }

            SetBroom(broomType);

        }

        private void SetBroom(BroomType broom)
        {
            InventoryManager.EquippedBroom = broom;
            if (InventoryManager.EquippedBroomChanged)
            {
                SoundManager.PlaySoundEffect("menu_move");
                UpdateBroomIcon();
                _player.broom.UpdateBroomType();
            }
        }

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
            if (InventoryManager.EquippedBroom == BroomType.NONE)
            {
                return;
            }

            string tex = "";

            BroomType broom = InventoryManager.EquippedBroom;

            switch (broom)
            {
                case BroomType.Normal:
                    tex = GlobalState.UseCellBroom ? "Cell" : "Normal";
                    break;
                case BroomType.Wide:
                    tex = "Wide";
                    break;
                case BroomType.Long:
                    tex = "Long";
                    break;
                case BroomType.Transformer:
                    tex = "Transform";
                    break;
            }

            _equippedBroomIcon = ResourceManager.GetTexture("hud" + tex, true);
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

            foreach (EntityPreset p in EntityManager.GetMapEntities(GlobalState.CURRENT_MAP_NAME).Where(p => p.Permanence == Permanence.MAP_LOCAL))
            {
                p.Alive = true;
            }

            LoadGridEntities();

            _keyValueLabel.SetText($"x{InventoryManager.GetCurrentMapKeys()}");

            PlayMapMusic();

            UpdateBroomIcon();

            if (GlobalState.GameMode != GameMode.Normal)
            {
                ReloadMapTextures();
            }
        }

        private void ReloadMapTextures()
        {
            _map.ReloadTexture();
            _map_bg_2.ReloadTexture();
            _map_fg.ReloadTexture();
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
            else if ((!GlobalState.WindmillOpened && title == "windmill") || GlobalState.InDeathRoom || title == "debug")
            {
                SoundManager.StopSong();
                return;
            }

            SoundManager.PlaySong(title);
        }

        private IEnumerable<Entity> SubEntities(Entity e)
        {
            return Enumerable.Repeat(e, 1).Concat(e.SubEntities().SelectMany(s => SubEntities(s)));
        }

        private void LoadGridEntities()
        {
            _oldEntities = new List<Entity>(_gridEntities);

            List<EntityPreset> gridPresets = EntityManager.GetGridEntities(GlobalState.CURRENT_MAP_NAME, new Vector2(GlobalState.CURRENT_GRID_X, GlobalState.CURRENT_GRID_Y));
            foreach (EntityPreset preset in gridPresets.Where(e => e.Permanence == Permanence.GRID_LOCAL))
            {
                preset.Alive = true;
            }

            GlobalState.ENEMIES_KILLED = gridPresets.Where(preset => !preset.Alive && preset.Type.IsDefined(typeof(EnemyAttribute), false)).Count();
            GlobalState.PUZZLES_SOLVED = 0;

            _groups = new CollisionGroups(GlobalState.ENEMIES_KILLED);
            _groups.Register(_player);
            _groups.Register(_player.broom);

            _gridEntities = gridPresets.Where(preset => preset.Alive)
                .Select(preset => preset.Create(_player)).SelectMany(e => SubEntities(e)).ToList();
            foreach (Entity e in _gridEntities)
            {
                _groups.Register(e);
            }


        }
    }
}
