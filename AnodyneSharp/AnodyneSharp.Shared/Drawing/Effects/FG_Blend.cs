using AnodyneSharp.Registry;
using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Drawing.Effects
{
    static class FG_Blend
    {
        static private Effect effect;
        static private bool active = false;

        public static void Load(ContentManager content)
        {
            effect = content.Load<Effect>("effects/fg_blendlayer");
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, 160, 160, 0, 0, -1));
            effect.Parameters["OverlayZ"].SetValue(DrawingUtilities.GetDrawingZ(DrawOrder.FG_SPRITES, 0));
        }

        public static void Update(Camera c)
        {
            effect.Parameters["View"].SetValue(c.Transform);
        }

        public static void MapChange()
        {
            active = true;
            switch (GlobalState.CURRENT_MAP_NAME)
            {
                case "SUBURB":
                    effect.Parameters["OverlayTex"].SetValue(ResourceManager.GetTexture("suburb_fg_overlay"));
                    effect.Parameters["HardLight"].SetValue(false);
                    break;
                case "HOTEL":
                    //TODO: only activate when on ROOF
                    effect.Parameters["OverlayTex"].SetValue(ResourceManager.GetTexture("hotel_roof_overlay"));
                    effect.Parameters["HardLight"].SetValue(true);
                    break;
                case "FOREST":
                    effect.Parameters["OverlayTex"].SetValue(ResourceManager.GetTexture("forest_fg_overlay"));
                    effect.Parameters["HardLight"].SetValue(false);
                    break;
                default:
                    active = false;
                    break;
            }
        }

        public static Effect GetEffect()
        {
            return active ? effect : null;
        }
    }
}
