﻿using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("Water_Anim")]
    public class WaterAnim : Entity
    {
        public WaterAnim(EntityPreset preset, Player p) : base(preset.Position)
        {
            if(GlobalState.events.GetEvent(preset.TypeValue) != 0)
            {
                var s = DoWaterAnim(preset.Position);
                while (s.MoveNext());
            }
            exists = false;
        }

        static public IEnumerator<string> DoWaterAnim(Vector2 position)
        {
            Point p = GlobalState.Map.ToMapLoc(position);

            Point grid = new(p.X/10,p.Y/10);

            bool InGrid(Point p)
            {
                return (p / new Point(10, 10)) == grid;
            }
            Point invert = new(-1, -1);

            int current = GlobalState.Map.GetTile(MapData.Layer.BG2, p);
            bool horiz = (current % 2) == 0;

            Point dir = horiz switch
            {
                true => new((p.X+1)/10 == grid.X ? 1 : -1,0),
                false => new(0, (p.Y+1)/10 == grid.Y ? 1 : -1)
            };


            while(true)
            {
                GlobalState.Map.ChangeTile(MapData.Layer.BG, p, current);
                p += dir;
                if(!InGrid(p) || GlobalState.Map.GetTile(MapData.Layer.BG2, p) == 0)
                    break;

                if(InGrid(p+dir) && GlobalState.Map.GetTile(MapData.Layer.BG2, p + dir) == 0)
                {
                    Point next_dir = new(dir.Y, dir.X);
                    if(GlobalState.Map.GetTile(MapData.Layer.BG2, p + next_dir) == 0)
                    {
                        next_dir *= invert;
                    }
                    if(GlobalState.Map.GetTile(MapData.Layer.BG2, p +next_dir) != 0)
                    {
                        dir = next_dir;
                        current -= current % 2;
                        if (dir.Y != 0) current++;
                    }
                }
                yield return null;
            }

            yield break;
        }
    }
}
