using AnodyneSharp.Drawing;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Windmill
{
    public record OpenedWindmill : GameEvent { };

    [NamedEntity("NPC", type: "generic", 3)]
    public class Shell : Entity
    {
        Blade[] blades;

        public Shell(EntityPreset preset, Player p)
            : base(new Vector2(160 + 32, 320 + 48), "windmill_shell", 48, 48, DrawOrder.FG_SPRITES)
        {
            Vector2 center = Position;

            blades = new Blade[]
            {
                new Blade(center , 0, 0),
                new Blade(center, 270, 8),
                new Blade(center , 180, 16)
            };
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return blades;
        }

        [Events(typeof(OpenedWindmill), typeof(EndScreenTransition))]
        private class Blade : Entity
        {
            int baseAngle;
            int swapIndex;

            Vector2 _center;

            public Blade(Vector2 center, int angle, int startFrame)
                : base(center, "windmill_blade", 192, 192, DrawOrder.FOOT_OVERLAY)
            {
                baseAngle = angle;

                _center = center;

                SetFrame(startFrame);

                swapIndex = 24 - startFrame;
                AddAnimation("rotate", Enumerable.Range(0, 32).Select(i => (startFrame + i) % 24).ToArray(), 20);

                visible = false;

                if (GlobalState.events.GetEvent("WindmillOpened") != 0)
                {
                    Play("rotate");
                }
            }

            public override void OnEvent(GameEvent e)
            {
                base.OnEvent(e);

                if (e is OpenedWindmill)
                {
                    Play("rotate");
                }
                else if (e is EndScreenTransition)
                {
                    visible = true;
                }
            }

            public override void PostUpdate()
            {
                base.PostUpdate();

                if (CurAnimIndex >= swapIndex)
                {
                    SetPos((baseAngle + 270) % 360);
                }
                else
                {
                    SetPos(baseAngle);
                }
            }


            private void SetPos(int angle)
            {
                rotation = MathHelper.ToRadians(angle);

                switch (angle)
                {
                    case 0:
                        Position = new Vector2(_center.X, _center.Y - 192 + 48);
                        break;
                    case 90:
                        Position = new Vector2(_center.X, _center.Y);
                        break;
                    case 180:
                        Position = new Vector2(_center.X - 192 + 48, _center.Y);
                        break;
                    case 270:
                        Position = new Vector2(_center.X - 192 + 48, _center.Y - 192 + 48);
                        break;
                }
            }
        }
    }
}
