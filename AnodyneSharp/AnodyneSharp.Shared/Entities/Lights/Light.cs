using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Lights
{
    public class Light : Entity
    {
        public Light(Vector2 pos, string textureName, int frameWidth, int frameHeight) : base(pos, textureName, frameWidth, frameHeight, DrawOrder.ENTITIES)
        {
            offset = new Vector2(frameWidth / 2, frameHeight / 2);
            scale = 3;
        }

        public override void Draw()
        {
            GlobalState.darkness.AddLight(this);
        }

        public void DrawLight()
        {
            base.Draw();
        }
    }

    public class FiveFrameGlowLight : Light
    {
        public FiveFrameGlowLight(Vector2 pos) : base(pos, "5-frame-glow", 48, 48)
        {
            AddAnimation("glow", CreateAnimFrameArray(0, 0, 1, 2, 3, 4, 3, 2, 1, 0, 0, 0), 7);
            Play("glow");
        }
    }

}
