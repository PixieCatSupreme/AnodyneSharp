using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity, Collision(typeof(Player), typeof(Dust), typeof(Stop_Marker))]
    public class Propelled : Entity
    {
        Entity poof;

        Vector2 initial_pos;

        bool pressed = false;
        bool pressedThisFrame = false;
        bool active;
        bool startDir = true;

        public Propelled(EntityPreset preset, Player p) : base(preset.Position, "moving_platform", 16, 16, Drawing.DrawOrder.VERY_BG_ENTITIES)
        {
            width = height = 10;
            CenterOffset();
            initial_pos = preset.Position;

            poof = new(Vector2.Zero, "moving_platform_poof", 16, 16, Drawing.DrawOrder.BG_ENTITIES);
            poof.AddAnimation("play", CreateAnimFrameArray(0, 1, 2, 3, 4), 12, false);
            poof.SetFrame(4);

            facing = (preset.Frame % 4) switch
            {
                0 => Facing.UP,
                1 => Facing.RIGHT,
                2 => Facing.DOWN,
                _ => Facing.LEFT
            };

            active = preset.Frame >= 4;
            UpdateFrame();
        }

        public override void Update()
        {
            base.Update();

            if (pressed && !pressedThisFrame)
            {
                SoundManager.PlaySoundEffect("button_up");
            }
            else if (!pressed && pressedThisFrame)
            {
                SoundManager.PlaySoundEffect("button_down");
            }
            pressed = pressedThisFrame;
            UpdateFrame();

            if (!startDir)
            {
                PotentialStop(initial_pos);
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            pressedThisFrame = false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(poof, 1);
        }

        void UpdateFrame()
        {
            SetFrame(((FacingDirection(facing).Y != 0) ? 4 : 0) + (pressed ? 2 : 0) + (active ? 1 : 0));
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            switch (other)
            {
                case Player p:
                    p.DontFall();
                    if (p.state == PlayerState.GROUND)
                    {
                        pressedThisFrame = true;
                        if (p.just_landed && active && velocity == Vector2.Zero)
                        {
                            velocity = FacingDirection(facing) * 33;
                            active = false;
                            poof.Play("play");
                            Vector2 poofCenter = VisualCenter - FacingDirection(facing) * 16;
                            poof.Position = poofCenter - new Vector2(poof.width / 2, poof.height / 2);
                            SoundManager.PlaySoundEffect("dustpoof");
                        }
                        p.additionalVel += velocity;
                    }
                    break;
                case Dust d:
                    if (!active)
                    {
                        active = true;
                        d.Play("poof");
                        SoundManager.PlaySoundEffect("dash_pad_2");
                    }
                    break;
                case Stop_Marker s:
                    if (startDir)
                    {
                        PotentialStop(s.Position);
                    }
                    break;
            }
        }

        void PotentialStop(Vector2 pos)
        {
            if (velocity != Vector2.Zero && (Position - offset - pos).LengthSquared() < 4)
            {
                velocity = Vector2.Zero;
                Position = pos + offset;
                FaceTowards(Position - FacingDirection(facing));
                startDir = !startDir;
            }
        }
    }

    [NamedEntity]
    public class Stop_Marker : Entity
    {
        public Stop_Marker(EntityPreset preset, Player p) : base(preset.Position, 16, 16, Drawing.DrawOrder.BG_ENTITIES)
        {
            visible = false;
        }
    }
}
