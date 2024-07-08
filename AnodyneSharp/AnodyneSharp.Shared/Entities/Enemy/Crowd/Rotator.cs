using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Crowd
{
    [NamedEntity, Collision(typeof(Player))]
    public class Rotator : Entity
    {
        public const string DamageDealer = "Rotator";
        RotatorBullet bullet;

        public Rotator(EntityPreset preset, Player p) : base(preset.Position, new AnimatedSpriteRenderer("f_rotator", 16, 16, new Anim("pulse", new int[] { 0, 1 }, 10)), Drawing.DrawOrder.BG_ENTITIES)
        {
            immovable = true;

            width = 6;
            height = 5;
            offset = new(5, 2);
            Position += offset;

            bullet = new(this);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(bullet, 1);
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }
    }

    [Collision(typeof(Player))]
    public class RotatorBullet : Entity
    {
        float radius = 0;
        float angle = 0;

        Rotator parent;

        public RotatorBullet(Rotator parent) : base(parent.Position, new AnimatedSpriteRenderer("f_rotator_ball", 8, 8, new Anim("pulse", new int[] { 0, 1 }, 10)), Drawing.DrawOrder.ENTITIES)
        {
            width = height = 4;
            CenterOffset();
            immovable = true;

            this.parent = parent;
        }

        public override void Update()
        {
            base.Update();
            MathUtilities.MoveTo(ref radius, 59, 30);

            angle += 1.2f * GameTimes.DeltaTime;
            if (angle > MathF.Tau)
                angle -= MathF.Tau;

            Position = parent.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (other is Player p && p.state != PlayerState.AIR)
            {
                p.ReceiveDamage(1, Rotator.DamageDealer);
            }
        }
    }
}
