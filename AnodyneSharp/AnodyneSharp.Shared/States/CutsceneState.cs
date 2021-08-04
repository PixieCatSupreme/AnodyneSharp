using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Effects;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Entities;
using AnodyneSharp.Map;
using AnodyneSharp.Map.Tiles;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.States
{
    public class CutsceneState : State
    {
        Camera _camera;

        MapLayer _map;

        List<Entity> _entities = new();

        float oldDarkness = GlobalState.darkness.Alpha;

        IEnumerator<CutsceneEvent> _state;

        DialogueState _substate;

        public CutsceneState(Camera camera, IEnumerator<CutsceneEvent> state)
        {
            _camera = camera;
            _state = state;
        }

        public override void Update()
        {
            base.Update();
            if(_substate is not null)
            {
                _substate.Update();
                if(_substate.Exit)
                {
                    _substate = null;
                }
            }
            else if(_state.MoveNext())
            {
                CutsceneEvent e = _state.Current;
                if(e != null)
                {
                    if(e is WarpEvent w)
                    {
                        Warp(w.Map, w.Grid);
                    }
                    else if(e is EntityEvent entity)
                    {
                        _entities.AddRange(entity.NewEntities);
                    }
                    else if(e is DialogueEvent d)
                    {
                        GlobalState.Dialogue = d.Diag;
                        _substate = new();
                    }
                }
            }
            else
            {
                Exit = true;
                _camera.GoTo(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid)); //reset camera
                GlobalState.darkness.MapChange(GlobalState.CURRENT_MAP_NAME);
                GlobalState.darkness.ForceAlpha(oldDarkness);
                GlobalState.staticEffect.MapChange(GlobalState.CURRENT_MAP_NAME);
                GlobalState.extraBlend.MapChange(GlobalState.CURRENT_MAP_NAME);
                FG_Blend.MapChange(GlobalState.CURRENT_MAP_NAME);
            }
            foreach(Entity e in _entities.Where(e=>e.exists))
            {
                e.Update();
                e.PostUpdate();
            }
        }

        public void Warp(string map, Point grid)
        {
            Spritesheet tiles = TileData.GetTileset(map);
            _map = new MapLayer();
            _map.LoadMap(MapLoader.GetMapLayer(map), tiles, DrawOrder.MAP_BG);
            _camera.GoTo(grid.ToVector2() * 160);
            DrawPlayState = false;
            UpdateEntities = false;
            GlobalState.staticEffect.MapChange(map);
            GlobalState.darkness.MapChange(map);
            GlobalState.extraBlend.MapChange(map);
            FG_Blend.MapChange(map);
        }

        public override void Draw()
        {
            base.Draw();
            if (_map != null)
            {
                _map.Draw(_camera.Bounds);
            }
            foreach(Entity e in _entities)
            {
                e.Draw();
            }
        }

        public override void DrawUI()
        {
            base.DrawUI();
            if(_substate != null)
            {
                _substate.DrawUI();
            }
        }

        public abstract record CutsceneEvent { };
        public sealed record DialogueEvent(string Diag) : CutsceneEvent { };
        public sealed record WarpEvent(string Map, Point Grid) : CutsceneEvent { };
        public sealed record EntityEvent(IEnumerable<Entity> NewEntities) : CutsceneEvent { };
    }
}
