using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Entities.Enemy.Go
{
    class HappyThorn : BigThorn
    {
        IceCrystal crystal;

        public HappyThorn(Vector2 pos, ILayerType layer) : base(pos, "briar_arm_left", 0, layer)
        {
            offset = new(4, 4);
            Position += offset;
            crystal = new(this);
        }

        public override IEnumerator GetAttacked(BigThorn blue, Player player)
        {
            int phase = 3 - Health;
            int number_back_and_forth = 2 + phase;

            crystal.exists = true;
            crystal.Position = blue.Position + new Vector2(5, 52);
            crystal.state = crystal.DoAttack(number_back_and_forth, 50 + phase * 20, player);

            while (crystal.state is not null) yield return null;

            yield break;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(crystal, 1);
        }

        [Collision(typeof(Player), typeof(Broom))]
        public class IceCrystal : Entity
        {
            EntityPool<IceExplosion> explosions = new(5, () => new());
            HappyThorn parent;

            public IEnumerator state;

            bool hit_player, hit_broom;

            public IceCrystal(HappyThorn thorn) : base(Vector2.Zero, new AnimatedSpriteRenderer("briar_ice_crystal", 16, 16, new Anim("move",new int[] { 0, 1 },6)), Drawing.DrawOrder.ENTITIES)
            {
                parent = thorn;
                exists = false;
            }

            public override void Update()
            {
                base.Update();
                if (!state?.MoveNext() ?? false)
                {
                    state = null;
                }
                hit_player = hit_broom = false;
            }

            public IEnumerator DoAttack(int n, float base_vel, Player player)
            {
                Vector2 thorn_target_loc = parent.Position + new Vector2(6, 52);

                Flicker(1.5f);
                while (_flickering) yield return null;

                explosions.Spawn(s => s.Spawn(this));
                SoundManager.PlaySoundEffect("teleguy_down");

                float vel_mul = 1;

                for (int i = 0; i < n; ++i)
                {
                    MoveTowards(player.Position, base_vel * vel_mul);
                    while (!(hit_broom || hit_player))
                    {
                        if (!Hitbox.Intersects(GlobalState.ScreenHitbox)) state = Wait();
                        yield return null;
                    }
                    explosions.Spawn(s => s.Spawn(this));
                    GlobalState.screenShake.Shake(0.01f, 0.2f);
                    if (hit_player)
                    {
                        player.ReceiveDamage(1, BriarBossMain.IceDamageDealer);
                        state = Wait();
                        yield return null; //yield break would set state to null
                    }
                    if (hit_broom)
                    {
                        SoundManager.PlaySoundEffect("broom_hit");
                        vel_mul += 0.15f;
                        MoveTowards(thorn_target_loc, base_vel * vel_mul);
                    }

                    while ((Position - thorn_target_loc).LengthSquared() > 25) yield return null;

                    GlobalState.screenShake.Shake(0.01f, 0.2f);
                    explosions.Spawn(s => s.Spawn(this));
                    SoundManager.PlaySoundEffect("sb_ball_appear");

                    vel_mul += 0.15f;
                }

                parent.Play("hurt");
                visible = false;

                while (parent.CurAnimName == "hurt") yield return null;

                IEnumerator wait = Wait();
                while (wait.MoveNext()) yield return null;

                yield break;
            }

            IEnumerator Wait()
            {
                while (explosions.Alive > 0) yield return null;
                exists = false;
                velocity = Vector2.Zero;
                yield break;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                if (other is Player)
                {
                    hit_player = true;
                }
                else if (other is Broom)
                {
                    hit_broom = true;
                }
            }

            public override IEnumerable<Entity> SubEntities()
            {
                return explosions.Entities;
            }


        }

        public class IceExplosion : Entity
        {
            public IceExplosion() : base(Vector2.Zero, new AnimatedSpriteRenderer("briar_ice_explosion", 24, 24, new Anim("explode",new int[] { 0, 1, 2, 3 },15,false)), Drawing.DrawOrder.FG_SPRITES)
            {
            }

            public override void Update()
            {
                base.Update();
                if (AnimFinished)
                    exists = false;
            }

            public void Spawn(IceCrystal parent)
            {
                Spawn(parent.Position - Vector2.One * 4);
            }

            public void Spawn(Vector2 pos)
            {
                Position = pos;
                Play("explode");
            }
        }
    }
}
