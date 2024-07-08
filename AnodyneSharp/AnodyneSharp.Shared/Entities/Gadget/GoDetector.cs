using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("Go_Detector")]
    public class GoDetector : Entity
    {
        List<Point> checks;
        EntityPreset _preset;
        public GoDetector(EntityPreset preset, Player p) : base(preset.Position)
        {
            visible = false;
            _preset = preset;
            checks = preset.TypeValue.Split(';').Select((s) =>
            {
                int[] vals = s.Split(',').Select(int.Parse).ToArray();
                return new Point(vals[0], vals[1]);
            }).ToList();
        }

        public override void Update()
        {
            base.Update();
            Point b = GlobalState.Map.ToMapLoc(Position);
            for(int i = 0; i < checks.Count; ++i)
            {
                if(GlobalState.Map.GetTile(MapData.Layer.BG,b+checks[i]) != 60+i)
                {
                    return;
                }
            }
            GlobalState.PUZZLES_SOLVED++;
            _preset.Alive = exists = false;
        }
    }
}
