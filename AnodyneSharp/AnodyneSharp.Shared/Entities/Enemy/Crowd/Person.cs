using AnodyneSharp.Entities.Base.Rendering;
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

        public static AnimatedSpriteRenderer GetSprite() => new("person", 16, 16,
            new Anim("walk_d",new int[] { 0, 1 }, 5),
            new Anim("walk_r", new int[] { 2, 3 }, 5),
            new Anim("walk_u", new int[] { 4, 5 }, 5),
            new Anim("walk_l", new int[] { 6, 7 }, 5)
            );

        public Person(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            height = 4;
            width = 6;

            CenterOffset();

            offset.Y += 4;

            talk_timer = (float)(0.5 + GlobalState.RNG.NextDouble());

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
            facing = direction;
            PlayFacing("walk");

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
