using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Treasures
{
    public class EmptyTreasureEvent : GameEvents.GameEvent { }

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
            : base(frame != 48 ?"card_sheet" : "card_49", pos, 24, 24, frame)
        {
            animTimer = 0;
            scale = 0.5f;
            offset = new Vector2( 12);
            Position.X += 9;
        }

        public override void GetTreasure()
        {
            if(GlobalState.inventory.CardStatus[_curAnim.Frame])
            {
                exists = false;
                GlobalState.FireEvent(new EmptyTreasureEvent());
                return;
            }
            base.GetTreasure();

            GlobalState.inventory.CardStatus[_curAnim.Frame] = true;

            DebugLogger.AddInfo($"Got card {_curAnim.Frame}");

            GlobalState.achievements.CheckCardAchievements();
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
                    velocity.Y = -24;
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
                    opacity -= GameTimes.DeltaTime * 0.5f;
                    if (opacity <= 0)
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
