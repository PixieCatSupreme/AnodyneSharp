using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;
using System;

namespace AnodyneSharp.Entities
{
    public enum ShadowType
    {
        Normal,
        Big,
        Tiny,
        RollerHorizontal,
        RollerVertical
    }

    public class Shadow : Entity
    {
        private Entity _parent;
        private int maxFrame = 0;

        public Shadow(Entity parent, Vector2 offset, ShadowType type = ShadowType.Normal, float fps = 8)
            : base(parent.Position, DrawOrder.SHADOWS)
        {
            _parent = parent;
            this.offset = offset - new Vector2(_parent.sprite.Width / 4, _parent.sprite.Height / 4);

            switch (type)
            {
                case ShadowType.Normal:
                    SetTexture("8x8_shadow", 8, 8);
                    maxFrame = 3;
                    break;
                case ShadowType.Big:
                    SetTexture("28x10_shadow", 28, 10);
                    maxFrame = 4;
                    break;
                case ShadowType.Tiny:
                    SetTexture("teeny_shadow", 3, 3);
                    break;
                case ShadowType.RollerHorizontal:
                    SetTexture("spike_roller_horizontal_shadow", 128, 16);
                    break;
                case ShadowType.RollerVertical:
                    SetTexture("spike_roller_shadow", 16, 128);
                    break;
            }
        }

        //jumpHeight between 0 and 1, how high up we are
        public void UpdateShadow(float jumpHeight)
        {
            if(maxFrame > 0)
            {
                int frame = (int)MathF.Ceiling(jumpHeight * (maxFrame+1)) - 1;
                if(frame == -1)
                {
                    visible = false;
                }
                else
                {
                    visible = true;
                    SetFrame(frame);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            Position = _parent.Position;
        }
    }
}
