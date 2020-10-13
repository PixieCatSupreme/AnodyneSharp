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
            this.offset = offset - new Vector2(_parent.sprite.Width / 4, _parent.sprite.Height / 4);

            switch (type)
            {
                case ShadowType.Normal:
                    SetTexture("8x8_shadow", 8, 8);

                    AddAnimation("get_big", CreateAnimFrameArray(0, 1, 2, 3), fps, false);
                    AddAnimation("get_small", CreateAnimFrameArray(3, 2, 1, 0), fps, false);
                    break;
                case ShadowType.Big:
                    SetTexture("28x10_shadow", 28, 10);

                    AddAnimation("get_big", CreateAnimFrameArray(0, 1, 2, 3, 4), fps, false);
                    AddAnimation("get_small", CreateAnimFrameArray(4, 3, 2, 1, 0), fps, false);
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

        public override void Update()
        {
            base.Update();

            Position = _parent.Position;
        }
    }
}
