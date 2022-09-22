using AnodyneSharp.Cheatz;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Effects;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Entities;
using AnodyneSharp.Entities.Enemy;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Input;
using AnodyneSharp.MapData;
using AnodyneSharp.MapData.Tiles;
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
using System.Linq.Expressions;
using static AnodyneSharp.Registry.GameConstants;

namespace AnodyneSharp.States
{
    public enum PlayStateState
    {
        S_LOAD,
        S_NORMAL,
        S_TRANSITION,
        S_MAP_EXIT,
        S_MAP_ENTER
    }

    public class PlayState : State
    {
        public const string UiHeader = "header";

        public const float Scroll_Speed = 250;


        private float transition_out
        {
            get
            {
                return GlobalState.CURRENT_MAP_NAME == "BLANK" ? 1.568f : 0.785f;
            }
        }
        private const float transition_in = 0.8f;
        private const float pixelation_per_second = 30f;

        private PlayStateState _state = PlayStateState.S_LOAD;

        private Map _map;

        private Player _player;

        private Camera _camera;

        private Rectangle _gridBorders;

        private HealthBar _healthBar;
        private Texture2D _header;

        private CollisionGroups _groups; //Deals with entity-map and entity-entity collision
        private EntityEventRegistry _eventRegistry;

        private List<Entity> _gridEntities; //Holds entities that stay on the current grid coordinate
        private List<Entity> _oldEntities; //Holds entities that will despawn after a screen transition is complete

        private List<Entity> _newlySpawned = new(); //Entities that are spawned during update

        private List<State> _childStates = new();

        private UILabel _keyValueLabel;
        private Spritesheet _miniminimap;

        private Texture2D _equippedBroomBorder;
        private Texture2D _equippedBroomIcon;

        private DustIcon _dustIcon;


        private Vector2 _iconPos;

        private float _texRandomTimer;
        private ScrollingTex _background;
        private ScrollingTex _dec_over;

        private Action _map_specific_update;

        private bool DoQuickLoad = false;

        public PlayState(Camera camera)
        {
            _gridEntities = new List<Entity>();
            _oldEntities = new List<Entity>();

            _camera = camera;

            _player = new Player();
            _healthBar = new HealthBar();

            _iconPos = new Vector2(2, 3);

            _dustIcon = new DustIcon(new Vector2(10, 10), _player.broom);

            CreateKeyLabel();

            GlobalState.SpawnEntity = SpawnEntity;
            GlobalState.FireEvent = FireEvent;
            GlobalState.SetSubstate = s => _childStates.Add(s); //Only allow setting state if there is no child state currently

            GlobalState.DoQuickSave = QuickSave;
            GlobalState.DoQuickLoad = () => DoQuickLoad = true;
        }

        private void QuickSave()
        {
            SoundManager.PlaySoundEffect("menu_select");

            GlobalState.serialized_quicksave = new GlobalState.Save().ToString();
            GlobalState.quicksave_checkpoint = new(GlobalState.CURRENT_MAP_NAME, _player.Position);
        }

        private void QuickLoad()
        {
            if (GlobalState.serialized_quicksave is null)
            {
                return;
            }

            SoundManager.PlaySoundEffect("menu_select");

            GlobalState.Save s = GlobalState.Save.FromString(GlobalState.serialized_quicksave);
            GlobalState.ResetValues();
            GlobalState.LoadSave(s);
            GlobalState.PLAYER_WARP_TARGET = GlobalState.quicksave_checkpoint.Position;
            GlobalState.NEXT_MAP_NAME = GlobalState.quicksave_checkpoint.map;
            ChangeStateEvent(AnodyneGame.GameState.Game);
        }

        private void FireEvent(GameEvent e)
        {
            _eventRegistry.FireEvent(e);
        }

        private void SpawnEntity(Entity t)
        {
            _newlySpawned.AddRange(SubEntities(t));
        }

        public override void Create()
        {
            base.Create();

            _header = ResourceManager.GetTexture(UiHeader, true);

            _equippedBroomBorder = ResourceManager.GetTexture("frame_icon", true);

            _miniminimap = new Spritesheet(ResourceManager.GetTexture("mini_minimap_tiles", true), 5, 5);

            GlobalState.WARP = true;
            Warp();

            _state = PlayStateState.S_MAP_ENTER;
            GlobalState.pixelation.SetPixelation(10);
            GlobalState.black_overlay.ForceAlpha(1);
        }

        public override void Draw()
        {
            if (_childStates.All(s => s.DrawPlayState))
            {
                if (_background != null)
                    _background.Draw(_camera);
                if (_dec_over != null)
                    _dec_over.Draw(_camera);

                _map.Draw(_camera.Bounds);

                _player.Draw();

                foreach (Entity gridEntity in _gridEntities)
                {
                    gridEntity.Draw();
                }
                foreach (Entity gridEntity in _oldEntities)
                {
                    gridEntity.Draw();
                }
            }
            foreach (var s in _childStates)
            {
                s.Draw();
            }
        }

        public override void DrawUI()
        {
            SpriteDrawer.DrawGuiSprite(_header, Vector2.Zero, Z: DrawingUtilities.GetDrawingZ(DrawOrder.HEADER));


            if (GlobalState.inventory.EquippedBroom != BroomType.NONE && _equippedBroomIcon != null)
            {
                SpriteDrawer.DrawGuiSprite(_equippedBroomIcon, _iconPos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.UI_OBJECTS));
            }

            SpriteDrawer.DrawGuiSprite(_equippedBroomBorder, _iconPos, Z: DrawingUtilities.GetDrawingZ(DrawOrder.EQUIPPED_BORDER));

            _dustIcon.Draw();

            _healthBar.Draw();

            foreach (var s in _childStates)
            {
                s.DrawUI();
            }


            _keyValueLabel.Draw();

            Minimap mm = GlobalState.CurrentMinimap;
            if (mm.tiles.Width > 0)
            {
                int x = Math.Clamp(GlobalState.CURRENT_GRID_X - 2, 0, mm.tiles.Width - 5);
                int y = Math.Clamp(GlobalState.CURRENT_GRID_Y - 1, 0, mm.tiles.Height - 4);

                mm.Draw(_miniminimap, new Vector2(55, 0) - new Vector2(x, y) * _miniminimap.Width, new Rectangle(x, y, 5, 4));
            }

            foreach (var UiEntity in GlobalState.UIEntities)
            {
                UiEntity.Draw();
            }

            GlobalState.UIEntities.Clear();
        }

        public override void Update()
        {
            base.Update();

            if (_background != null) _background.Update();
            if (_dec_over != null) _dec_over.Update();

            _dustIcon.Update();
            _dustIcon.PostUpdate();

            foreach (Entity e in _newlySpawned)
            {
                _gridEntities.Add(e);
                _groups.Register(e);
                _eventRegistry.Register(e);
            }
            _newlySpawned.Clear();

            bool updateEntities = _childStates.All(s => s.UpdateEntities);

            if (_childStates.Count > 0)
            {
                _player.invincible = true;
                _player.dontMove = true;
                _player.actions_disabled = true;

                foreach (var s in _childStates)
                {
                    s.Update();
                    if (s.Exit && s is DeathState)
                    {
                        Warp();
                        _state = PlayStateState.S_MAP_ENTER;
                    }
                }
                _childStates.RemoveAll(s => s.Exit);
                if (_childStates.Count == 0)
                {
                    _player.dontMove = false;
                    _player.exists = true;
                    _player.actions_disabled = false;
                    _player.invincible = false;
                    _player.skipBroom = true;
                }

                if (updateEntities)
                {
                    DoCollisions(false);
                }
            }
            else
            {
                var oldstate = _state;
                switch (_state)
                {
                    case PlayStateState.S_NORMAL:
                        StateNormal();
                        break;
                    case PlayStateState.S_TRANSITION:
                        //Registry.sound_data.current_song.volume = FlxG.volume * Registry.volume_scale;
                        StateTransition();
                        break;
                    case PlayStateState.S_MAP_EXIT:
                        _player.invincible = true;
                        _player.dontMove = true;
                        _player.actions_disabled = true;
                        GlobalState.pixelation.AddPixelation(pixelation_per_second);
                        GlobalState.black_overlay.ChangeAlpha(1 / transition_out);
                        if (GlobalState.CURRENT_MAP_NAME != GlobalState.NEXT_MAP_NAME)
                        {
                            SoundManager.SetMasterVolume(MathF.Min(1, 10 * (1 - GlobalState.black_overlay.alpha)));
                        }
                        if (GlobalState.black_overlay.alpha == 1 || GlobalState.FUCK_IT_MODE_ON)
                        {
                            Warp();

                            //Warp resets player values
                            _player.invincible = true;
                            _player.dontMove = true;
                            _player.actions_disabled = true;

                            _state = PlayStateState.S_MAP_ENTER;
                            GlobalState.pixelation.SetPixelation(10f);
                        }
                        break;
                    case PlayStateState.S_MAP_ENTER:
                        if (GlobalState.FUCK_IT_MODE_ON)
                        {
                            GlobalState.pixelation.SetPixelation(1f);
                        }
                        GlobalState.pixelation.AddPixelation(-pixelation_per_second);
                        GlobalState.black_overlay.ChangeAlpha(-1 / transition_in);
                        if (GlobalState.black_overlay.alpha == 0)
                        {
                            _state = PlayStateState.S_NORMAL;
                            _player.dontMove = false;
                            _player.invincible = false;
                            _player.actions_disabled = false;
                            GlobalState.WARP = false;
                        }
                        if (SoundManager.GetMasterVolume() != 1f)
                        {
                            SoundManager.SetMasterVolume(MathF.Min(10 * (1 - GlobalState.black_overlay.alpha), 1));
                        }
                        break;
                    default:
                        break;
                }
                //no player-map collision during screen transition or during the one normal state frame in wiggle glitch
                DoCollisions(oldstate == PlayStateState.S_TRANSITION || _state == PlayStateState.S_TRANSITION);
            }

            if (updateEntities)
            {
                EntityUpdate();
            }

            if (GlobalState.SetDialogueMode)
            {
                _childStates.Add(new DialogueState(GlobalState.Dialogue));
                GlobalState.SetDialogueMode = false;
                _player.BeIdle();
            }
            if (GlobalState.StartCutscene != null)
            {
                _childStates.Add(new CutsceneState(_camera, GlobalState.StartCutscene) { ChangeStateEvent = ChangeStateEvent});
                GlobalState.StartCutscene = null;
            }

#if DEBUG
            DebugKeyInput();
#endif

            _map.Update();

            Refreshes();

            _map_specific_update?.Invoke();

            //This needs to be at the end in order not to mess with the initialisation of the new playstate
            if (DoQuickLoad)
            {
                QuickLoad();
            }
        }

        private void Refreshes()
        {
            if (GlobalState.inventory.EquippedBroomChanged)
            {
                GlobalState.inventory.EquippedBroomChanged = false;

                UpdateBroomIcon();
            }

            if (GlobalState.RefreshLabels)
            {
                CreateKeyLabel();
                GlobalState.RefreshKeyCount = true;
            }

            if (GlobalState.RefreshMaxHealth)
            {
                GlobalState.RefreshMaxHealth = false;
                _healthBar.CreateHealthBoxes(GlobalState.MAX_HEALTH);
            }

            UpdateHealth();
            _healthBar.Update();

            if (GlobalState.RefreshKeyCount)
            {
                GlobalState.RefreshKeyCount = false;
                _keyValueLabel.SetText($"x{GlobalState.inventory.GetCurrentMapKeys()}");
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

                _player.Reset(false);
                _player.ReloadTexture();
                _map.ReloadTexture();

                foreach (var item in _gridEntities)
                {
                    item.ReloadTexture();
                }

            }

            GlobalState.ENEMIES_KILLED = _groups.KilledEnemies();
        }

        private void EntityUpdate()
        {
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Accept) && CheckInteraction())
            {
                _player.skipBroom = true;
            }

            foreach (Entity gridEntity in _gridEntities.Where(e => e.exists))
            {
                gridEntity.Update();
                gridEntity.PostUpdate();
            }

            _player.Update();
            _player.PostUpdate();
        }

        private bool CheckInteraction()
        {
            if (_player.state != PlayerState.GROUND || _player.skipBroom || _childStates.Count > 0) //skipBroom can be set before interaction check for other reasons, like going out of a dialogue state
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

            return _gridEntities.OfType<Interactable>().Any(i => (((Entity)i).exists && InteractHitbox((Entity)i).Intersects(_player.Hitbox) && i.PlayerInteraction(_player.facing)));
        }

        private void DoCollisions(bool ignore_player)
        {
            _groups.DoCollision(_map, ignore_player);
        }

        private void StateNormal()
        {
            if (GlobalState.ToTitle)
            {
                GlobalState.ToTitle = false;
                ChangeStateEvent(AnodyneGame.GameState.TitleScreen);

                GlobalState.CURRENT_MAP_NAME = "";
                return;
            }

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.Pause) && !GlobalState.disable_menu)
            {
                _childStates.Add(new PauseState() { ChangeStateEvent = ChangeStateEvent });
                SoundManager.PlaySoundEffect("pause_sound");
                return;
            }

            if (GlobalState.WARP)
            {
                _state = PlayStateState.S_MAP_EXIT;
                _eventRegistry.FireEvent(new StartWarp());
                _map.OnTransitionStart();
                return;
            }

            CheckForTransition();

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

                if (KeyInput.JustPressedRebindableKey(KeyFunctions.Broom1))
                {
                    SetBroom(BroomType.Normal);
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Broom2))
                {
                    SetBroom(BroomType.Long);
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Broom3))
                {
                    SetBroom(BroomType.Wide);
                }
                else if (KeyInput.JustPressedRebindableKey(KeyFunctions.Broom4))
                {
                    SetBroom(BroomType.Transformer);
                }
            }

            if (KeyInput.JustPressedRebindableKey(KeyFunctions.QuickSave))
            {
                QuickSave();
            }
            if (KeyInput.JustPressedRebindableKey(KeyFunctions.QuickLoad))
            {
                DoQuickLoad = true; //Need to wait for end of update to do this
            }
        }

        private void CheckForTransition()
        {
            _state = PlayStateState.S_TRANSITION;
            Point grid = new(GlobalState.CURRENT_GRID_X, GlobalState.CURRENT_GRID_Y);
            if (_player.Position.X < _gridBorders.X)
            {
                grid.X--;
                _player.Position.X = _gridBorders.X - _player.width;
            }
            else if (_player.Position.Y < _gridBorders.Y)
            {
                grid.Y--;

                _player.Position.Y = _gridBorders.Y - _player.height;
            }
            else if (_player.Position.Y > _gridBorders.Bottom - _player.height)
            {
                grid.Y++;
                _player.Position.Y = _gridBorders.Y + _gridBorders.Height;
            }
            else if (_player.Position.X > _gridBorders.Right - _player.width)
            {
                grid.X++;
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
                GlobalState.ScreenTransition = true;
                OnGridExit();
                GlobalState.CURRENT_GRID_X = grid.X;
                GlobalState.CURRENT_GRID_Y = grid.Y;
                _eventRegistry.FireEvent(new StartScreenTransition());
                _map.OnTransitionStart();
                _map.ReloadSettings(_player.Position);
                _player.grid_entrance = _player.Position;
                _player.dontMove = true;

                _player.broom.dust = null;

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

                _player.dontMove = false;

                FinalizeTransition();

                GameTimes.TimeScale = (1/60f) / GameTimes.DeltaTime;
                _state = PlayStateState.S_NORMAL;
                GlobalState.ScreenTransition = false;
                _eventRegistry.FireEvent(new EndScreenTransition());
            }
        }

        private void FinalizeTransition()
        {
            //delete old objects
            _map.OnTransitionEnd();
            _oldEntities.Clear();
        }

        private void CreateKeyLabel()
        {
            _keyValueLabel = new UILabel(new Vector2(37, 5), true, "", new Color(124, 163, 177, 255),forceEnglish:true);
        }

        private void UpdateScreenBorders()
        {
            _gridBorders.X = GlobalState.CURRENT_GRID_X * SCREEN_WIDTH_IN_PIXELS;
            _gridBorders.Width = SCREEN_WIDTH_IN_PIXELS;
            _gridBorders.Y = GlobalState.CURRENT_GRID_Y * SCREEN_HEIGHT_IN_PIXELS;
            _gridBorders.Height = SCREEN_HEIGHT_IN_PIXELS;

            GlobalState.CurrentMinimap.Update();
        }

        private bool UpdateCamera()
        {
            if (GlobalState.FUCK_IT_MODE_ON)
            {
                _camera.GoTo(_gridBorders.X, _gridBorders.Y);
                return false;
            }

            return !_camera.GoTowards(new(_gridBorders.X, _gridBorders.Y), Scroll_Speed);
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

            if (KeyInput.JustPressedKey(Keys.F10))
            {
                Cheatz.Cheatz.KonamiCode();
            }
            if (KeyInput.JustPressedKey(Keys.F9))
            {
                Cheatz.Cheatz.UnlockNexusGates();
            }

            if (KeyInput.JustPressedKey(Keys.NumPad5))
            {
                GlobalState.draw_hitboxes = !GlobalState.draw_hitboxes;
            }

            if (KeyInput.JustPressedKey(Keys.M))
            {
                int newIndex = MapUtilities.GetMapID(GlobalState.CURRENT_MAP_NAME) + 1;

                if (newIndex > (int)MapOrder.DEBUG)
                {
                    newIndex = 0;
                }

                GlobalState.NEXT_MAP_NAME = Enum.GetName(typeof(MapOrder), (MapOrder)newIndex);
                GlobalState.PLAYER_WARP_TARGET = Vector2.Zero;
                GlobalState.WARP = true;

            }
            else if (KeyInput.JustPressedKey(Keys.N))
            {
                int newIndex = MapUtilities.GetMapID(GlobalState.CURRENT_MAP_NAME) - 1;

                if (newIndex < (int)MapOrder.STREET)
                {
                    newIndex = (int)MapOrder.DEBUG;
                }

                GlobalState.NEXT_MAP_NAME = Enum.GetName(typeof(MapOrder), (MapOrder)newIndex);
                GlobalState.PLAYER_WARP_TARGET = Vector2.Zero;
                GlobalState.WARP = true;
            }

            if (KeyInput.JustPressedKey(Keys.D5))
            {
                SetBroom(BroomType.NONE);
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
                //testing out map offset instead of camera
                float camSpeed = !GlobalState.FreeRoamCamera ? SCREEN_WIDTH_IN_PIXELS : 70;
                camSpeed *= GameTimes.DeltaTime;

                Vector2 offset = new();
                if (KeyInput.IsKeyPressed(Keys.NumPad4))
                {
                    offset += -Vector2.UnitX * camSpeed;
                }
                else if (KeyInput.IsKeyPressed(Keys.NumPad6))
                {
                    offset += Vector2.UnitX * camSpeed;
                }

                if (KeyInput.IsKeyPressed(Keys.NumPad8))
                {
                    offset += -Vector2.UnitY * camSpeed;
                }
                else if (KeyInput.IsKeyPressed(Keys.NumPad2))
                {
                    offset += Vector2.UnitY * camSpeed;
                }
                _map.offset += offset;

                if (KeyInput.JustPressedKey(Keys.NumPad0))
                {
                    _map.offset = Vector2.Zero;
                }
            }
        }
#endif
        private void SwitchBroom(bool nextBroom)
        {
            if (!GlobalState.inventory.HasBroom)
            {
                return;
            }

            BroomType broomType = GlobalState.inventory.EquippedBroom;

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
                } while (!GlobalState.inventory.HasBroomType(broomType));
            }

            SetBroom(broomType);

        }

        private void SetBroom(BroomType broom)
        {
            GlobalState.inventory.EquippedBroom = broom;
            if (GlobalState.inventory.EquippedBroomChanged)
            {
                SoundManager.PlaySoundEffect("menu_move");
                UpdateBroomIcon();
                _player.broom.UpdateBroomType();

                if (broom != BroomType.Transformer)
                {
                    _player.transformer.Reset();
                }
            }
        }

        private void UpdateHealth()
        {
            _healthBar.UpdateHealth();

            if (_childStates.Count == 0 && GlobalState.CUR_HEALTH == 0)
            {
                SoundManager.StopSong();

                _childStates.Add(new DeathState(_player));
            }
        }

        private void UpdateBroomIcon()
        {
            if (GlobalState.inventory.EquippedBroom == BroomType.NONE)
            {
                return;
            }

            string tex = "";

            BroomType broom = GlobalState.inventory.EquippedBroom;

            switch (broom)
            {
                case BroomType.Normal:
                    tex = GlobalState.IsCell ? "Cell" : GlobalState.IsKnife && GlobalState.events.GetEvent("Stabbed") != 0 ? "Knife" : "Normal";
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


        private void Warp()
        {
            if (GlobalState.CURRENT_MAP_NAME != GlobalState.NEXT_MAP_NAME)
            {

                GlobalState.events.VisitedMaps.Add(GlobalState.NEXT_MAP_NAME);
                if (GlobalState.events.BossDefeated.Contains(GlobalState.CURRENT_MAP_NAME))
                {
                    GlobalState.events.LeftAfterBoss.Add(GlobalState.CURRENT_MAP_NAME);
                }

                if (_state != PlayStateState.S_LOAD)
                {
                    foreach (EntityPreset p in EntityManager.GetMapEntities(GlobalState.CURRENT_MAP_NAME).Where(p => p.Permanence != Permanence.GLOBAL))
                    {
                        p.Alive = true;
                    }

                    GlobalState.PillarSwitchOn = 0;
                }

                GlobalState.CURRENT_MAP_NAME = GlobalState.NEXT_MAP_NAME;

                if (GlobalState.ReturnTarget == null || GlobalState.ReturnTarget.map != GlobalState.CURRENT_MAP_NAME || GlobalState.CURRENT_MAP_NAME == "NEXUS")
                {
                    var gate = EntityManager.GetNexusGateForCurrentMap();
                    GlobalState.ReturnTarget = (gate != null) ? new(gate) : null;
                }

                GlobalState.Map = _map = new(GlobalState.CURRENT_MAP_NAME);

                GlobalState.MAP_GRID_WIDTH = _map.WidthInTiles / 10;
                GlobalState.MAP_GRID_HEIGHT = _map.HeightInTiles / 10;


                UpdateBroomIcon();

                if (GlobalState.GameMode != GameMode.Normal)
                {
                    _map.ReloadTexture();
                }


                GlobalState.RefreshKeyCount = true;
            }

            _player.Position = _player.grid_entrance = GlobalState.PLAYER_WARP_TARGET;
            _player.facing = GlobalState.NewMapFacing ?? _player.facing;

            _map.ReloadSettings(_player);

            GlobalState.NewMapFacing = null;

            Point gridPos = MapUtilities.GetRoomCoordinate(_player.Position);
            Vector2 roomPos = MapUtilities.GetRoomUpperLeftPos(gridPos);

            GlobalState.CURRENT_GRID_X = gridPos.X;
            GlobalState.CURRENT_GRID_Y = gridPos.Y;

            _player.Reset();

            _camera.GoTo(roomPos);

            UpdateScreenBorders();

            SetBackground();
            SetDecOver();
            SetMapUpdate();

            LoadGridEntities();

            FinalizeTransition();
        }

        private void SetBackground()
        {
            _background = GlobalState.CURRENT_MAP_NAME switch
            {
                "BLANK" => new ScrollingTex("BLANK_BG", new Vector2(-20, 0), DrawOrder.BACKGROUND),
                "SPACE" => new ScrollingTex("SPACE_BG", new Vector2(-15, 0), DrawOrder.BACKGROUND),
                "GO" => new ScrollingTex("briar_BG", new Vector2(0, 15), DrawOrder.BACKGROUND),
                "NEXUS" => new ScrollingTex("nexus_bg", new Vector2(0, 15), DrawOrder.BACKGROUND),
                _ => null,
            };
        }

        private void SetDecOver()
        {
            _dec_over = GlobalState.CURRENT_MAP_NAME switch
            {
                "WINDMILL" => new ScrollingTex("windmill_rain", new Vector2(-10, 160), DrawOrder.DEC_OVER),
                _ => null,
            };
        }

        private void SetMapUpdate()
        {
            _map_specific_update = GlobalState.CURRENT_MAP_NAME switch
            {
                "BLANK" => () =>
                {
                    if (GlobalState.RNG.NextDouble() < 0.05) GlobalState.screenShake.Shake(0.007f, 0.1f);
                    if (GlobalState.RNG.NextDouble() < 0.005)
                    {
                        GlobalState.darkness.ForceAlpha(0.1f);
                        GlobalState.darkness.TargetAlpha(0f);
                    }
                }
                ,
                _ => null
            };
        }

        private IEnumerable<Entity> SubEntities(Entity e)
        {
            return Enumerable.Repeat(e, 1).Concat(e.SubEntities().SelectMany(s => SubEntities(s)));
        }

        private void OnGridExit()
        {
            foreach (EntityPreset preset in EntityManager.GetGridEntities(GlobalState.CURRENT_MAP_NAME, new Point(GlobalState.CURRENT_GRID_X, GlobalState.CURRENT_GRID_Y)))
            {
                if (preset.Permanence == Permanence.GRID_LOCAL)
                {
                    preset.Alive = true;
                }
            }
        }

        private void LoadGridEntities()
        {
            _oldEntities = _gridEntities;

            List<EntityPreset> gridPresets = EntityManager.GetGridEntities(GlobalState.CURRENT_MAP_NAME, new Point(GlobalState.CURRENT_GRID_X, GlobalState.CURRENT_GRID_Y));
            if (_state != PlayStateState.S_LOAD)
            {
                foreach (EntityPreset preset in gridPresets.Where(e => e.Permanence == Permanence.GRID_LOCAL))
                {
                    preset.Alive = true;
                }
            }

            GlobalState.ENEMIES_KILLED = gridPresets.Where(preset => !preset.Alive && preset.Type.IsDefined(typeof(EnemyAttribute), false)).Count();
            GlobalState.PUZZLES_SOLVED = 0;

            _groups = new CollisionGroups(GlobalState.ENEMIES_KILLED);
            _eventRegistry = new();

            IEnumerable<Entity> player_subentities = _player.SubEntities(); //Cache first since some entities add themselves to the player on creation

            _gridEntities = gridPresets.Where(preset => preset.Alive || preset.Type.IsDefined(typeof(AlwaysSpawnAttribute), true))
                .Select(preset => preset.Create(_player)).Concat(player_subentities).SelectMany(e => SubEntities(e)).ToList();

            _groups.Register(_player);
            _eventRegistry.Register(_player);

            foreach (Entity e in _gridEntities)
            {
                _groups.Register(e);
                _eventRegistry.Register(e);
            }

        }
    }
}
