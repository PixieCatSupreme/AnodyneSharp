using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class CardTreasure : BaseTreasure
    {
        private enum AnimState
        {
            Delay,
            Grow,
            Hang,
            GrowSpinFade
        }

        AnimState state;
        float animTimer;

        public CardTreasure(Vector2 pos, int frame)
            : base("card_sheet", pos, 24, 24, frame)
        {
            animTimer = 0;
            scale = 0.5f;
        }

        public override void GetTreasure()
        {
            base.GetTreasure();

            InventoryState.CardStatus[_curFrame] = true;

            DebugLogger.AddInfo($"Got card {_curFrame}");
        }

        public override void Update()
        {
            base.Update();

            if (exists)
            {
                GrowthAnim();
            }

            base.PostUpdate();
        }

        private void GrowthAnim()
        {
            switch (state)
            {
                case AnimState.Delay:

                    state++;
                    velocity.Y = 24;
                    if (Position.Y <= GlobalState.CURRENT_GRID_Y * 160 + 80)
                    {
                        velocity.Y *= -1;
                    }
                    break;
                case AnimState.Grow:
                    scale += 0.5f * GameTimes.DeltaTime;

                    if (scale >= 1)
                    {
                        scale = 1;
                        velocity = Vector2.Zero;
                        state++;
                    }
                    break;
                case AnimState.Hang:
                    animTimer += GameTimes.DeltaTime;
                    if (animTimer > 1)
                    {
                        state++;

                        acceleration.Y = -80;
                        angularVelocity = MathHelper.ToRadians(50);
                        angularAcceleration = MathHelper.ToRadians(200);

                        if (Position.Y <= GlobalState.CURRENT_GRID_Y * 160 + 80)
                        {
                            acceleration.Y *= -1;
                            angularVelocity *= -1;
                            angularAcceleration *= -1;
                        }
                    }
                    break;
                case AnimState.GrowSpinFade:
                    scale += GameTimes.DeltaTime * 3f;
                    _opacity -= GameTimes.DeltaTime * 0.5f;
                    if (_opacity <= 0)
                    {
                        visible = false;
                        state++;
                    }
                    break;
                default:
                    exists = false;
                    break;
            }

        }
    }
}
