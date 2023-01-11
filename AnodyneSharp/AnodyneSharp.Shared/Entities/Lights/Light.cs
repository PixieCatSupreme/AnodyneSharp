using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Light(Vector2 pos, AnimatedSpriteRenderer sprite) : base(pos,sprite,DrawOrder.ENTITIES) {
            offset = new Vector2(sprite.Width / 2, sprite.Height / 2);
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

    public class ConeLight : Light
    {
        public ConeLight(Vector2 pos) : base(pos, "cone-light", 32, 32)
        {
            scale = 0.2f;
        }
    }

    public class FiveFrameGlowLight : Light
    {
        public FiveFrameGlowLight(Vector2 pos) : base(pos, new("5-frame-glow", 48, 48, new Anim("glow",new int[] { 0, 0, 1, 2, 3, 4, 3, 2, 1, 0, 0, 0 },7)))
        {
        }
    }

    public class BrightGlowLight : Light
    {
        public BrightGlowLight(Vector2 pos) : base(pos, new("5-frame-glow-bright", 48, 48, new Anim("glow", Enumerable.Range(1, 19).ToArray(), 7, false)))
        {
        }
    }

    public class PlayerLight : Light
    {
        Player _player;
        const float flicker_timer_max = 6/30f;
        float flicker_timer = flicker_timer_max;

        public PlayerLight(Player p) : base(p.Position,"player-light",64,64)
        {
            _player = p;
            scale = 2;
        }

        public override void Update()
        {
            base.Update();
            Position = _player.Center;
            flicker_timer -= GameTimes.DeltaTime;
            if(flicker_timer < 0f)
            {
                if(scale != 2f)
                {
                    scale = 2f;
                    flicker_timer = flicker_timer_max;
                }
                else
                {
                    scale += (float)(GlobalState.RNG.NextDouble() * 0.2);
                    flicker_timer = 0.05f;
                }
            }
        }
    }

    [NamedEntity("Event", "bedroom_bounce"), Collision(typeof(Player), KeepOnScreen = true)]
    public class BedRoomBounceLight : Light
    {
        private float lock_on_timer = 2.0f;
        private bool locked_on = false;
        private Player followee;

        public BedRoomBounceLight(EntityPreset preset, Player p) : base(p.Position, "glow-light", 64, 64)
        {
            scale = 2;
            followee = p;
            width = height = 32;

            GlobalState.PlayerLight = this; //some entities need to change this light's properties

            velocity = Vector2.One * 30f;
            int vel_decider = GlobalState.RNG.Next(0, 4);
            if (vel_decider % 2 == 0)
            {
                velocity.X *= -1;
            }
            if (vel_decider / 2 == 0)
            {
                velocity.Y *= -1;
            }
            
            if (GlobalState.events.BossDefeated.Contains("BEDROOM"))
            {
                preset.Alive = exists = false;
                return;
            }
        }

        public override void Update()
        {
            base.Update();


            if (locked_on)
            {
                MathUtilities.MoveTo(ref Position.X, followee.Position.X, 30f);
                MathUtilities.MoveTo(ref Position.Y, followee.Position.Y, 30f);
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
