using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy
{
    [NamedEntity, Collision(typeof(Player), MapCollision = true)]
    public class Person : Entity
    {
        private const int speed = 10;

        private float switch_dir_timer_max = 1.3f;
        private float switch_dir_timer = 2f;

        private float talk_timer;

        public Person(EntityPreset preset, Player p)
            : base(preset.Position, "person", 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            height = 2;
            width = 4;
            offset = new Vector2( 7, 12);

            talk_timer = (float)(0.5 + GlobalState.RNG.NextDouble());

            AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 5);
            AddAnimation("walk_r", CreateAnimFrameArray(2, 3), 5);
            AddAnimation("walk_u", CreateAnimFrameArray(4, 5), 5);
            AddAnimation("walk_l", CreateAnimFrameArray(2, 3), 5);

            switch (preset.Frame)
            {
                case 0:
                    FaceDirection(Facing.DOWN);
                    break;
                case 1:
                    FaceDirection(Facing.RIGHT);
                    break;
                case 2:
                    FaceDirection(Facing.UP);
                    break;
                case 3:
                    FaceDirection(Facing.LEFT);
                    break;
                case 4:
                    velocity = new Vector2(GlobalState.RNG.Next(-speed, speed));
                    if (velocity.X > 0)
                    {
                        Play("walk_r");
                    }
                    else
                    {
                        Play("walk_l");
                    }
                    switch_dir_timer_max = 0.5f;

                    SetFlip();
                    break;
            }

        }

        public override void Update()
        {
            base.Update();

            switch_dir_timer -= GameTimes.DeltaTime;

            if (switch_dir_timer < 0)
            {
                switch_dir_timer = switch_dir_timer_max;

                FaceDirection((Facing)GlobalState.RNG.Next(0, 4));
            }

            talk_timer -= GameTimes.DeltaTime;

            if (talk_timer < 0)
            {
                talk_timer = (float)(0.5 + GlobalState.RNG.NextDouble());
                SoundManager.PlaySoundEffect("talk_1", "talk_1", "talk_2", "talk_3", "talk_3");
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Player p)
            {
                p.slowMul = 0.5f;
                p.slowTicks = 7;

                Separate(this, p);
            }
        }

        private void FaceDirection(Facing direction)
        {
            Play($"walk_{Enum.GetName(direction.GetType(), direction).ToLower()[0]}");

            velocity = FacingDirection(direction) * speed;

            SetFlip();
        }

        private void SetFlip()
        {
            if (velocity.X < 0)
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            }
        }
    }
}
