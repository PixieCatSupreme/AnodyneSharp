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

    [NamedEntity("Event","bedroom_bounce"), Collision(typeof(Player),KeepOnScreen = true)]
    public class BedRoomBounceLight : Light
    {
        private float lock_on_timer = 2.0f;
        private bool locked_on = false;
        private Player followee;
        private EntityPreset preset;

        public BedRoomBounceLight(EntityPreset preset, Player p) : base(p.Position,"glow-light",64,64)
        {
            scale = 2;
            followee = p;
            this.preset = preset;
            width = height = 32;

            velocity = Vector2.One * 30f;
            int vel_decider = GlobalState.RNG.Next(0, 4);
            if(vel_decider % 2 == 0)
            {
                velocity.X *= -1;
            }
            if(vel_decider/2 == 0)
            {
                velocity.Y *= -1;
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (EventRegister.BossDefeated.Contains("BEDROOM"))
            {
                preset.Alive = exists = false;
                return;
            }

            if (locked_on)
            {
                MoveTowards(followee.Position, 30f);
            }
            else
            {
                if ((touching & (Touching.LEFT | Touching.RIGHT)) != Touching.NONE)
                {
                    velocity.X *= -1;
                }
                if ((touching & (Touching.UP | Touching.DOWN)) != Touching.NONE)
                {
                    velocity.Y *= -1;
                }

                velocity.X += (float)GlobalState.RNG.NextDouble() * 2 - 1;
                velocity.Y += (float)GlobalState.RNG.NextDouble() * 2 - 1;
                lock_on_timer -= GameTimes.DeltaTime;
            }
        }

        public override void Collided(Entity other)
        {
            if (lock_on_timer < 0) locked_on = true;
        }
    }

}
