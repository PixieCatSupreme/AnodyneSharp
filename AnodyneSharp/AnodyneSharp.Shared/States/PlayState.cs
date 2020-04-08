using AnodyneSharp.Map;
using AnodyneSharp.Map.Tiles;

namespace AnodyneSharp.States
{
    public class PlayState : State
    {
        private TileMap map;
        public TileMap map_bg_2; //on top of the bg map
        public TileMap map_fg; // on top of all sprites but below darkness

        private bool[] mapData;
        private bool fg_solid;

        public PlayState()
        {
            map = new TileMap();
            map_bg_2 = new TileMap();
            map_fg = new TileMap();
        }

        public override void Create()
        {
            base.Create();

            LoadMap();
        }

        public override void Draw()
        {
            base.Draw();

            map.Draw();
            //map_bg_2.Draw();
            //map_fg.Draw();
        }

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
