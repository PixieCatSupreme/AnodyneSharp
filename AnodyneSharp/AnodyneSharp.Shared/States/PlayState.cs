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
    public class PlayState : State
    {
        private TileMap map;
        private TileMap map_bg_2; //on top of the bg map
        private TileMap map_fg; // on top of all sprites but below darkness

        private bool[] mapData;
        private bool fg_solid;

        private Player player;

        private Camera _camera;

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

#if RELEASE
        }
#else
            DebugKeyInput();
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
                    _camera.GoTo(MapUtilities.GetCamRoomPos(new Vector2(1, 5)));
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

            _camera.GoTo(MapUtilities.AddCamOffset(Vector2.Zero));

            player.Position = MapUtilities.GetRoomUpperLeftPos(new Vector2(1, 5));
            player.Reset();

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
