using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Input;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Debug
{
    [NamedEntity]
    public class Black_Thing : Entity
    {
        BouncingBlackThing scale1, scale2;
        RotatingBlackThing thing3;

        Player player;
        int num_jumps = 0;

        public Black_Thing(EntityPreset preset, Player p) : base(preset.Position,
            new SolidColorRenderer(Color.Black, 32, 32), DrawOrder.ENTITIES)
        {
            player = p;
            opacity = 0.3f;
            scale1 = new(Position, Color.Red * 0.25f, 0.5f, 2f, 0.5f);
            scale2 = new(Position, Color.Green * 0.5f, 0.5f, 2f, 2f);
            thing3 = new(Position);
        }

        public override void Update()
        {
            if(Hitbox.Intersects(player.Hitbox) && KeyInput.JustPressedRebindableKey(KeyFunctions.Cancel))
            {
                SoundManager.PlaySoundEffect("fall_in_hole");
                num_jumps++;
                if(num_jumps == 50)
                {
                    scale1.maxScale = 10;
                    scale2.maxScale = 12;
                    thing3.maxScale = 9;
                    GlobalState.PUZZLES_SOLVED++;
                }
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { scale1, scale2, thing3 };
        }

        private class BouncingBlackThing : Entity
        {
            const float rate = 0.03f * 30;
            public float minScale, maxScale;
            float realScale;
            bool growing = true;

            public BouncingBlackThing(Vector2 pos, Color color, float min, float max, float current) : base(pos,
                new SolidColorRenderer(color,32,32),DrawOrder.ENTITIES)
            {
                realScale = current;
                minScale = min;
                maxScale = max;
            }

            public override void Update()
            {
                if(MathUtilities.MoveTo(ref realScale, growing ? maxScale : minScale, rate))
                {
                    growing = !growing;
                }
                //force centered rendering
                float width = realScale * sprite.Width;
                int evenWidth = (int)(width / 2) * 2;
                scale = (float)evenWidth / sprite.Width;
            }
        }

        private class RotatingBlackThing : Entity
        {
            public float minScale = 0.75f, maxScale = 3f;
            const float maxTime = 1f;
            float curTime = 0f;

            public RotatingBlackThing(Vector2 pos) : base(pos,
                new SolidColorRenderer(Color.Blue,32,32),DrawOrder.ENTITIES)
            {
                opacity = 0.7f;
            }

            public override void Update()
            {
                if (MathUtilities.MoveTo(ref curTime, maxTime, 0.5f))
                {
                    curTime = 0;
                }

                float lerpAmount = (1 + MathF.Sin(curTime * MathF.Tau)) / 2;

                scale = lerpAmount * (maxScale - minScale) + minScale;
                //Force centered rendering
                float width = scale * sprite.Width;
                int evenWidth = (int)(width / 2) * 2;
                scale = (float)evenWidth / sprite.Width;

                rotation = lerpAmount * MathF.PI - MathF.PI;
            }
        }
    }


}
