using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Drawing.Effects
{
    public class FG_Blend : Blend
    {
        public FG_Blend()
        {
            cutoff = DrawingUtilities.GetDrawingZ(DrawOrder.FG_SPRITES, 0);
        }

        public override void MapChange(string map)
        {
            switch (map)
            {
                case "SUBURB":
                    tex = ResourceManager.GetTexture("suburb_fg_overlay");
                    hard_light = false;
                    break;
                case "HOTEL_roof":
                    tex = ResourceManager.GetTexture("hotel_roof_fg_overlay");
                    hard_light = true;
                    break;
                case "FOREST":
                    tex = ResourceManager.GetTexture("forest_fg_overlay");
                    hard_light = false;
                    break;
                default:
                    tex = null;
                    break;
            }
        }
    }
}
