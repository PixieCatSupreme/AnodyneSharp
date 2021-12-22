using AnodyneSharp.Drawing;
using AnodyneSharp.Drawing.Effects;
using AnodyneSharp.Drawing.Spritesheet;
using AnodyneSharp.Entities;
using AnodyneSharp.MapData;
using AnodyneSharp.MapData.Tiles;
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

        Map _map;

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
                    else if(e is ReturnWarp)
                    {
                        Return();
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
                Return();
            }
            foreach(Entity e in _entities.Where(e=>e.exists))
            {
                e.Update();
                e.PostUpdate();
            }
        }

        public void Return()
        {
            _camera.GoTo(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid)); //reset camera
            (GlobalState.Map as Map).ReloadSettings(_camera.Position2D, graphics_only:true);
            GlobalState.darkness.ForceAlpha(oldDarkness);
            DrawPlayState = true;
            UpdateEntities = true;
            _map = null;
        }

        public void Warp(string map, Point grid)
        {
            _map = new(map);
            _camera.GoTo(grid.ToVector2() * 160);
            DrawPlayState = false;
            UpdateEntities = false;
            _map.ReloadSettings(_camera.Position2D, graphics_only:true);
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
        public sealed record ReturnWarp() : CutsceneEvent { };
        public sealed record EntityEvent(IEnumerable<Entity> NewEntities) : CutsceneEvent { };
    }
}
