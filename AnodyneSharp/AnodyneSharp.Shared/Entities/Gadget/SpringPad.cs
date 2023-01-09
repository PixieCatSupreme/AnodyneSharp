using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("Jump_Trigger", "1"), Collision(typeof(Player))]
    public class SpringPad : Entity
    {
        private int distance;
        private float time;

        private bool playerCollision;
        private bool activated;

        public static AnimatedSpriteRenderer GetSprite() => new("spring_pad", 16, 16,
            new Anim("still", new int[] { 0 },1),
            new Anim("pressed", new int[] { 1 },1),
            new Anim("wobble", new int[] { 0, 2, 0, 1, 0, 2, 0, 1, 0, 0 },10,false)
            );

        public SpringPad(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), DrawOrder.BG_ENTITIES)
        {
            switch (preset.Frame)
            {
                case 1:
                    distance = 48;
                    time = 0.5f;
                    break;
                case 2:
                    distance = 64;
                    time = 0.7f;
                    break;
                default:
                    distance = 32;
                    time = 0.3f;
                    break;
            }

            distance -= 14;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Player p)
            {
                playerCollision = true;

                if (p.JustLanded)
                {
                    activated = true;
                    SoundManager.PlaySoundEffect("spring_bounce");

                    Play("wobble");

                    p.facing = Facing.DOWN;

                    p.AutoJump(time, p.Position + new Vector2(0, distance));
                }
                else
                {
                    if (!activated && p.state == PlayerState.GROUND)
                    {
                        Play("pressed");
                    }
                    else if (p.state == PlayerState.AIR)
                    {
                        Play("still");
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (AnimFinished && !playerCollision)
            {
                activated = false;
                Play("still");
            }

            playerCollision = false;
        }
    }
}
