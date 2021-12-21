using AnodyneSharp.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Drawing.Effects
{
    public class Blend : IFullScreenEffect
    {
        protected Texture2D tex;
        protected bool hard_light = false;
        protected Effect blend;
        protected float cutoff = 1f;

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
            blend.Parameters["DepthCutoff"].SetValue(cutoff);
            batch.Begin(samplerState: SamplerState.PointClamp, effect: blend);
            batch.Draw(screen, screen.Bounds, Color.White);
            batch.End();
        }

        public void SetTex(string texName)
        {
            tex = ResourceManager.GetTexture(texName, allowUnknown: true);
            hard_light = texName.EndsWith("hardlight");
        }

        public virtual void MapChange(string mapName)
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
