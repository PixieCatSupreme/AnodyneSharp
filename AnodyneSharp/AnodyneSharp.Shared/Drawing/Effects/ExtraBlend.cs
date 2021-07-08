using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class ExtraBlend : IFullScreenEffect
    {
        private Texture2D tex;
        private bool hard_light = false;
        private Effect blend;

        public bool Active()
        {
            return tex != null;
        }

        public void Deactivate()
        {
        }

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            blend = content.Load<Effect>("effects/blend");
        }

        public void Render(SpriteBatch batch, Texture2D screen)
        {
            blend.Parameters["HardLight"].SetValue(hard_light);
            blend.Parameters["OverlayTex"].SetValue(tex);
            batch.Begin(samplerState: SamplerState.PointClamp, effect: blend);
            batch.Draw(screen, screen.Bounds, Color.White);
            batch.End();
        }

        public void MapChange(string mapName)
        {
            mapName = mapName.ToLower();
            tex = ResourceManager.GetTexture(mapName + "_extra_overlay", allowUnknown: true);
            if (tex != null)
            {
                hard_light = false;
            }
            else
            {
                tex = ResourceManager.GetTexture(mapName + "_extra_hardlight", allowUnknown: true);
                hard_light = true;
            }
        }
    }
}
