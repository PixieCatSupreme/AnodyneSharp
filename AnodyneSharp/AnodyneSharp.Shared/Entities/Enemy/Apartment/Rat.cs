using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Apartment
{
    [NamedEntity, Enemy, Collision(typeof(Player),typeof(Broom),typeof(DashTrap))]
    class Rat : HealthDropper
    {
        public const string DamageDealer = "Rat";
        Lookahead lookahead = new();

        public static AnimatedSpriteRenderer GetSprite(int o) => new("rat", 16, 16,
            new Anim("walk_d", new int[] { o + 0, o + 1 }, 5),
            new Anim("walk_l", new int[] { o + 2, o + 3 }, 5),
            new Anim("walk_r", new int[] { o + 2, o + 3 }, 5),
            new Anim("walk_u", new int[] { o + 4, o + 5 }, 5));

        public Rat(EntityPreset preset, Player p) : base(preset, preset.Position,GetSprite(GlobalState.IsCell ? 6 : 0),Drawing.DrawOrder.ENTITIES)
        {
            width = height = 12;
            CenterOffset();
            Position += offset;

            facing = Facing.DOWN;
            velocity = Vector2.UnitY * 40;

            lookahead.Position = Center + FacingDirection(facing) * 7;
        }

        public override void Update()
        {
            base.Update();

            if(lookahead.touching != Touching.NONE)
            {
                lookahead.touching = Touching.NONE;
                (velocity.X, velocity.Y) = (-velocity.Y, velocity.X);
                FaceTowards(Position + velocity);
                PlayFacing("walk");
                SoundManager.PlaySoundEffect("rat_move");
            }

            lookahead.Position = Center + FacingDirection(facing) * 7;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if(other is Player p)
            {
                if(new Rectangle(Hitbox.Left+4,Hitbox.Top+4,8,8).Intersects(p.Hitbox))
                {
                    p.ReceiveDamage(1, DamageDealer);
                }
            }
            else
            {
                GlobalState.SpawnEntity(new Explosion(this));
                Die();
            }
        }

        protected override void AnimationChanged(string name)
        {
            if(name.EndsWith("_l"))
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(Enumerable.Repeat(lookahead, 1));
        }

        [Collision(typeof(Dust), MapCollision = true)]
        class Lookahead : Entity
        {
            public Lookahead() : base(Vector2.Zero,1,1)
            {
                visible = false;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                touching = Touching.ANY;
            }

            public override void Puddle()
            {
                touching = Touching.ANY;
            }

            public override void Fall(Vector2 fallPoint)
            {
                touching = Touching.ANY;
            }

            public override void SlowTile()
            {
                touching = Touching.ANY;
            }

            public override void Reflection()
            {
                touching = Touching.ANY;
            }

            public override void Ladder()
            {
                touching = Touching.ANY;
            }

            public override void Conveyor(Touching direction)
            {
                touching = Touching.ANY;
            }

            public override void Grass()
            {
                touching = Touching.ANY;
            }
        }
    }
}
