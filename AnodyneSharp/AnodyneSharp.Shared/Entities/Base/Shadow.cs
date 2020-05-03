using AnodyneSharp.Drawing;
using Microsoft.Xna.Framework;

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

        public Shadow(Entity parent, Vector2 offset, ShadowType type = ShadowType.Normal, float fps = 8)
            : base(parent.Position, DrawOrder.SHADOWS)
        {
            _parent = parent;
            this.offset = offset;

            switch (type)
            {
                case ShadowType.Normal:
                    frameWidth = frameHeight = 8;

                    SetTexture("8x8_shadow");

                    AddAnimation("get_big", CreateAnimFrameArray(0, 1, 2, 3), fps, false);
                    AddAnimation("get_small", CreateAnimFrameArray(3, 2, 1, 0), fps, false);
                    break;
                case ShadowType.Big:
                    frameWidth = 28;
                    frameHeight = 10;

                    SetTexture("28x10_shadow");

                    AddAnimation("get_big", CreateAnimFrameArray(0, 1, 2, 3, 4), fps, false);
                    AddAnimation("get_small", CreateAnimFrameArray(4, 3, 2, 1, 0), fps, false);
                    break;
                case ShadowType.Tiny:
                    frameWidth = frameHeight = 3;

                    SetTexture("teeny_shadow");
                    break;
                case ShadowType.RollerHorizontal:
                    frameWidth = 128;
                    frameHeight = 16;

                    SetTexture("spike_roller_horizontal_shadow");
                    break;
                case ShadowType.RollerVertical:
                    frameWidth = 16;
                    frameHeight = 128;

                    SetTexture("spike_roller_shadow");
                    break;
            }
        }

        public override void Update()
        {
            base.Update();

            Position = _parent.Position;
        }
    }
}
