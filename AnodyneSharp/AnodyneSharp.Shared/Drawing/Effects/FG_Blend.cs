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
    }
}
