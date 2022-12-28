using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Redcave
{
    [NamedEntity, Collision(typeof(Player))]
    class Four_Shooter : Entity
    {
        const float tm = 1.5f;
        float timer = tm;

        EntityPool<Bullet> bullets = new(12, () => new());

        public Four_Shooter(EntityPreset preset, Player p) : base(preset.Position, "f_four_shooter", 16,16,Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("axis_to_diag", CreateAnimFrameArray(0, 1, 2), 3, false);
            AddAnimation("diag_to_axis", CreateAnimFrameArray(2, 1, 0), 3, false);
            SetFrame(0);
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return bullets.Entities;
        }

        public override void Update()
        {
            base.Update();
            timer -= GameTimes.DeltaTime;
            if(timer <= 0)
            {
                timer = tm;
                Sounds.SoundManager.PlaySoundEffect("4sht_shoot");
                if(GetFrame() == 0)
                {
                    Play("axis_to_diag");
                    bullets.Spawn((b) => b.Spawn(Center, Vector2.UnitX));
                    bullets.Spawn((b) => b.Spawn(Center, -Vector2.UnitX));
                    bullets.Spawn((b) => b.Spawn(Center, Vector2.UnitY));
                    bullets.Spawn((b) => b.Spawn(Center, -Vector2.UnitY));
                }
                else
                {
                    Play("diag_to_axis");
                    bullets.Spawn((b) => b.Spawn(Center, Vector2.One));
                    bullets.Spawn((b) => b.Spawn(Center, -Vector2.One));
                    bullets.Spawn((b) => b.Spawn(Center, Vector2.UnitY - Vector2.UnitX));
                    bullets.Spawn((b) => b.Spawn(Center, -Vector2.UnitY + Vector2.UnitX));
                }
            }
        }

        [Collision(typeof(Player),MapCollision = true)]
        class Bullet : Entity
        {
            public Bullet() : base(Vector2.Zero, "f_four_shooter_bullet", 8,8,Drawing.DrawOrder.FG_SPRITES)
            {
                AddAnimation("move",    CreateAnimFrameArray(0, 1), 12, true);
                AddAnimation("explode", CreateAnimFrameArray(2, 3), 10, false);
                MapInteraction = false;

                width = height = 4;
                offset = Vector2.One * 2;
            }

            public void Spawn(Vector2 center, Vector2 dir)
            {
                dir.Normalize();
                Position = center + dir * 8 - Vector2.One*width/2;
                velocity = dir * 50;
                Play("move");
            }

            public override void Update()
            {
                base.Update();
                if(CurAnimFinished)
                {
                    exists = false;
                    return;
                }
                if(touching != Touching.NONE && CurAnimName != "explode")
                {
                    Pop();
                }
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                if(other is Player p && p.state != PlayerState.AIR && CurAnimName != "explode")
                {
                    p.ReceiveDamage(1);
                    Pop();
                }
                
            }

            void Pop()
            {
                velocity = Vector2.Zero;
                Play("explode");
                Sounds.SoundManager.PlaySoundEffect("4sht_pop");
            }
        }
    }
}
