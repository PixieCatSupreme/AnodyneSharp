using AnodyneSharp.Drawing;
using AnodyneSharp.Input;
using AnodyneSharp.Map;
using AnodyneSharp.Map.Tiles;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework.Input;

namespace AnodyneSharp.States
{
    public class PlayState : State
    {
        private TileMap map;
        private TileMap map_bg_2; //on top of the bg map
        private TileMap map_fg; // on top of all sprites but below darkness

        private bool[] mapData;
        private bool fg_solid;

        private Camera _camera;

        public PlayState(Camera camera)
        {
            map = new TileMap();
            map_bg_2 = new TileMap();
            map_fg = new TileMap();

            _camera = camera;
        }

        public override void Create()
        {
            base.Create();

            LoadMap();
        }

        public override void Draw()
        {
            base.Draw();

            //Screens are 10x10 probably!
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
        }

        public override void Update()
        {
            base.Update();

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
                if (!GlobalState.FreeRoamCamera)
                {
                    float camSpeed = GameConstants.SCREEN_WIDTH_IN_TILES * 16;

                    if (KeyInput.CanPressKey(Keys.Left))
                    {
                        _camera.Move(-camSpeed, 0);
                    }
                    else if (KeyInput.CanPressKey(Keys.Right))
                    {
                        _camera.Move(camSpeed, 0);
                    }

                    if (KeyInput.CanPressKey(Keys.Up))
                    {
                        _camera.Move(0, -camSpeed);
                    }
                    else if (KeyInput.CanPressKey(Keys.Down))
                    {
                        _camera.Move(0, camSpeed);
                    }
                }
                else
                {

                    float camSpeed = KeyInput.IsKeyPressed(Keys.LeftShift) ? 8 : 4;

                    if (KeyInput.IsKeyPressed(Keys.Left))
                    {
                        _camera.Move(-camSpeed, 0);
                    }
                    else if (KeyInput.IsKeyPressed(Keys.Right))
                    {
                        _camera.Move(camSpeed, 0);
                    }

                    if (KeyInput.IsKeyPressed(Keys.Up))
                    {
                        _camera.Move(0, -camSpeed);
                    }
                    else if (KeyInput.IsKeyPressed(Keys.Down))
                    {
                        _camera.Move(0, camSpeed);
                    }
                }
            }
        }
#endif

        private void LoadMap()
        {
            TileData.SetTileset(Registry.GlobalState.CURRENT_MAP_NAME);
            map.LoadMap(MapLoader.GetMap(Registry.GlobalState.CURRENT_MAP_NAME), TileData.Tiles, 16, 16);
            mapData = map.GetData();

            //map_bg_2.null_buffer(0);
            //map_fg.null_buffer(0);

            map_bg_2.LoadMap(MapLoader.GetMap(Registry.GlobalState.CURRENT_MAP_NAME, 2), TileData.Tiles, 16, 16);
            map_bg_2.y = Registry.GameConstants.HEADER_HEIGHT;
            map_fg.LoadMap(MapLoader.GetMap(Registry.GlobalState.CURRENT_MAP_NAME, 3), TileData.Tiles, 16, 16);
            map_fg.y = Registry.GameConstants.HEADER_HEIGHT;

            _camera.Move(0, -GameConstants.HEADER_HEIGHT - GameConstants.TILE_HEIGHT * 2 + 2);

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
