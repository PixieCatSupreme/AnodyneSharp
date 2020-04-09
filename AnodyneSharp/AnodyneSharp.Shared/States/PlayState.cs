using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Player;
using AnodyneSharp.Input;
using AnodyneSharp.Map;
using AnodyneSharp.Map.Tiles;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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
        public int Scroll_Increment = 4;

        private PlayStateState state;

        private TileMap map;
        private TileMap map_bg_2; //on top of the bg map
        private TileMap map_fg; // on top of all sprites but below darkness

        private bool[] mapData;
        private bool fg_solid;

        private Player player;

        private Camera _camera;
        private bool justTransitioned;

        private Rectangle _gridBorders;

        public PlayState(Camera camera)
        {
            map = new TileMap();
            map_bg_2 = new TileMap();
            map_fg = new TileMap();

            _camera = camera;

            player = new Player();
        }

        public override void Create()
        {
            base.Create();

            LoadMap();
        }

        public override void Draw()
        {
            base.Draw();

#if DEBUG
            if (GlobalState.DrawBG)
            {
                map.Draw();
            }
            if (GlobalState.DrawBG2)
            {
                map_bg_2.Draw(0.1f, true);
            }
            if (GlobalState.DrawFG)
            {
                map_fg.Draw(0.2f, true);
            }

#else
            map.Draw();
            map_bg_2.Draw(0.1f, true);
            map_fg.Draw(0.2f, true);
#endif

            player.Draw();
        }

        public override void Update()
        {
            base.Update();

            player.Update();

            switch (state)
            {
                case PlayStateState.S_NORMAL:
                    StateNormal();
                    break;
                case PlayStateState.S_TRANSITION:
                    //Registry.sound_data.current_song.volume = FlxG.volume * Registry.volume_scale;
                    StateTransition();
                    //doCollisions();
                    return;
                case PlayStateState.S_PAUSED:
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
                    break;
                default:
                    break;
            }

#if RELEASE
        }
#else
            DebugKeyInput();
        }

        private void StateNormal()
        {
            CheckForTransition();

            //TODO add  pause check

            //TODO check if player is unalive
        }

        private void CheckForTransition()
        {
            state = PlayStateState.S_TRANSITION;
            if (player.Position.X < _gridBorders.X)
            {
                GlobalState.CURRENT_GRID_X--;
                player.Position.X = _gridBorders.X - player.frameWidth;
            }
            else if (player.Position.Y < _gridBorders.Y)
            {
                GlobalState.CURRENT_GRID_Y--;

                player.Position.Y = _gridBorders.Y - player.frameHeight;
            }
            else if (player.Position.Y > _gridBorders.Bottom - player.height)
            {
                GlobalState.CURRENT_GRID_Y++;
                player.Position.Y = _gridBorders.Y + _gridBorders.Height;
            }
            else if (player.Position.X > _gridBorders.Right - player.width)
            {
                GlobalState.CURRENT_GRID_X++;
                player.Position.X = _gridBorders.Right;
            }
            else
            {
                state = PlayStateState.S_NORMAL;
            }

            //debugText.text += " ub: " + upperBorder.toString() + "leb: " + leftBorder.toString() + "\n camx: " + FlxG.camera.bounds.x.toString() + "camy: " + FlxG.camera.bounds.y.toString() + 
            //"\n x: " + player.x.toFixed(2) + " y: " + player.y.toFixed(2);

            if (state ==  PlayStateState.S_TRANSITION)
            {
                player.grid_entrance_x = player.Position.X;
                player.grid_entrance_y = player.Position.Y;
                justTransitioned = true;
                player.dontMove = true;

                //TODO maybe put this in Player.cs to get the transition bug
                player.velocity = Vector2.Zero;
            }
        }

        private void StateTransition()
        {
            player.invincible = true;

            if (justTransitioned)
            {
                //TODO add enemy, puzzle and tile resetting on grid change
                UpdateScreenBorders();

                justTransitioned = false;
            }

            if (!UpdateCamera())
            {
                player.invincible = false;
                //TODO delete old objects

                // TODO update miniminimap

                //TODO reset broom

                player.dontMove = false;

                //TODO update minimap

                state = PlayStateState.S_NORMAL;
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
                _camera.Move(Scroll_Increment,0);
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

        private void DebugKeyInput()
        {

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

        private void LoadMap()
        {
            TileData.SetTileset(Registry.GlobalState.CURRENT_MAP_NAME);
            map.LoadMap(MapLoader.GetMap(Registry.GlobalState.CURRENT_MAP_NAME), TileData.Tiles);
            mapData = map.GetData();

            //map_bg_2.null_buffer(0);
            //map_fg.null_buffer(0);

            map_bg_2.LoadMap(MapLoader.GetMap(GlobalState.CURRENT_MAP_NAME, 2), TileData.Tiles);
            map_bg_2.y = HEADER_HEIGHT;
            map_fg.LoadMap(MapLoader.GetMap(GlobalState.CURRENT_MAP_NAME, 3), TileData.Tiles);
            map_fg.y = HEADER_HEIGHT;

            Vector2 gridPos = new Vector2(1,5);
            Vector2 roomPos = MapUtilities.GetRoomUpperLeftPos(gridPos);

            GlobalState.CURRENT_GRID_X = (int)gridPos.X;
            GlobalState.CURRENT_GRID_Y = (int)gridPos.Y;


            player.Position = roomPos + new Vector2(32,32);
            player.Reset();

            _camera.GoTo(roomPos);
            UpdateScreenBorders();

            //Sets tile collission and tile events
            //TileData.set_tile_properties(map_bg_2);
            //if (Registry.GlobalState.CURRENT_MAP_NAME == "OVERWORLD")
            //{
            //    TileData.set_tile_properties(map_fg);
            //    fg_solid = true;
            //}
        }
    }
}
