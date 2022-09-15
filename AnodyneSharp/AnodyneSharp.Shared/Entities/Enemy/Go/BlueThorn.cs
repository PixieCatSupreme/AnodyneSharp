using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AnodyneSharp.Entities.Enemy.Go
{
    class BlueThorn : BigThorn
    {
        FireEye shooter = new();

        public BlueThorn(Vector2 pos) : base(pos, "briar_arm_right")
        {
            AddAnimation("off", CreateAnimFrameArray(1, 2, 3, 0), 4);
            Play("off");
            offset.X = 35;
            offset.Y = 4;
            Position += offset;
        }

        public override IEnumerator GetAttacked(BigThorn blue, Player player)
        {
            float tm = 0;
            while (!MathUtilities.MoveTo(ref tm, 1.5f, 1)) yield return null;

            shooter.exists = true;
            shooter.Start(3 - Health, player);
            while (shooter.exists)
            {
                if(_curAnim.Finished)
                {
                    //Got hit by broom and need to end the attack
                    shooter.EndAttack();
                }
                yield return null;
            }

            yield break;
        }

        public void GetHit()
        {
            if(_curAnim.name != "hurt")
            {
                Play("hurt");
                GlobalState.screenShake.Shake(0.01f, 0.2f);
                SoundManager.PlaySoundEffect("wb_hit_ground");
                shooter.Stop();
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { shooter };
        }

        public class FireEye : Entity
        {
            IEnumerator state;
            int num_explosions = 0;
            EntityPool<Fireball> fireballs;
            EntityPool<Dust> dust = new(4, () => new());
            EntityPool<DustExplosion> explosions = new(4, ()=>new());
            EntityPool<Mist> mist = new(2, () => new());


            public FireEye() : base(Vector2.Zero,"briar_fire_eye",16,16,Drawing.DrawOrder.BG_ENTITIES)
            {
                fireballs = new(12, () => new(this));
                AddAnimation("shoot", CreateAnimFrameArray(6, 7, 8, 9), 10);
                AddAnimation("grow", CreateAnimFrameArray(0, 1, 2, 3, 4, 5), 10, false);
                AddAnimation("ungrow", CreateAnimFrameArray(5, 4, 3, 2, 1, 0), 10, false);
            }

            public void Start(int phase, Player p)
            {
                Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
                Play("grow");
                Position = tl + (phase == 0 ? new Vector2(70, 80) : new Vector2(50, 100));

                num_explosions = 0;

                dust.Spawn((d) => { d.exists = true; d.Position = tl + new Vector2(1, 7) * 16; d.Play("unpoof"); });
                dust.Spawn((d) => { d.exists = true; d.Position = tl + new Vector2(8, 7) * 16; d.Play("unpoof"); });
                dust.Spawn((d) => { d.exists = true; d.Position = tl + new Vector2(3, 5) * 16; d.Play("unpoof"); });
                dust.Spawn((d) => { d.exists = true; d.Position = tl + new Vector2(1, 5) * 16; d.Play("unpoof"); });

                if (phase == 1) {
                    //Original code looks like it should be spawning this one, but the second just overwrites it!
                    //mist.Spawn(m => m.Spawn(tl + new Vector2(6 * 16 - 7, 5 * 16 - 12)));
                    mist.Spawn(m => m.Spawn(tl + new Vector2(6 * 16 - 7, 7 * 16 - 12)));
                }
                else if(phase == 2)
                {
                    mist.Spawn(m => m.Spawn(tl + new Vector2(5 * 16 + 8, 4 * 16 + 5)));
                    mist.Spawn(m => m.Spawn(tl + new Vector2(6 * 16, 6 * 16 + 3)));
                }

                state = Attack(p);
            }

            IEnumerator Attack(Player p)
            {
                IEnumerator s = CoroutineUtils.OnceEvery(() => fireballs.Spawn((f) => f.Spawn(Position + Vector2.One * 4, p.Position)), 0.4f);

                while(num_explosions < 4 || explosions.Alive != 0)
                {
                    s.MoveNext();
                    yield return null;
                }

                state = End();
            }

            IEnumerator End()
            {
                foreach(var d in dust.Entities.Where(d=>d.exists))
                {
                    d.Play("poof");
                }
                foreach(var m in mist.Entities.Where(m=>m.exists))
                {
                    m.Flicker(1);
                }
                Play("ungrow");

                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 1.3f, 1)) yield return null;

                foreach(var m in mist.Entities)
                {
                    m.exists = false;
                }
                exists = false;
                yield break;
            }

            public void Stop()
            {
                state = null;
            }

            public void ExplodeAt(Dust d)
            {
                num_explosions++;
                d.exists = false;
                explosions.Spawn(e => e.Spawn(d));
                foreach(Facing f in Enum.GetValues(typeof(Facing)))
                {
                    fireballs.Spawn(e => e.SpawnFacing(d.Position, f));
                }
            }

            public void EndAttack()
            {
                state = End();
            }

            public override void Update()
            {
                base.Update();
                if (CurAnimName == "grow" && _curAnim.Finished) Play("shoot");
                state?.MoveNext();
            }

            public override IEnumerable<Entity> SubEntities()
            {
                return fireballs.Entities.Concat(dust.Entities).Concat(explosions.Entities).Concat(mist.Entities);
            }
        }

        [Collision(typeof(BlueThorn))]
        public class DustExplosion : Entity
        {
            public DustExplosion() : base(Vector2.Zero, "briar_dust_explosion", 48, 48, Drawing.DrawOrder.ENTITIES)
            {
                AddAnimation("explode", CreateAnimFrameArray(0, 1, 2, 3, 4, 5), 12, false);
            }

            public void Spawn(Dust d)
            {
                Play("explode");
                SoundManager.PlaySoundEffect("dust_explode");
                Position = d.Position - Vector2.One * 16;
            }

            public override void Update()
            {
                base.Update();
                if (_curAnim.Finished) exists = false;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                (other as BlueThorn).GetHit();
            }

        }

        [Collision(typeof(Player),typeof(Dust),typeof(Mist),KeepOnScreen = true)]
        public class Fireball : Entity
        {
            FireEye parent;

            public Fireball(FireEye parent) : base(Vector2.Zero, "briar_fire_eye", 16, 16, Drawing.DrawOrder.ENTITIES)
            {
                AddAnimation("move", CreateAnimFrameArray(12, 13), 12);
                Play("move");
                width = height = 6;
                CenterOffset();
                this.parent = parent;
            }

            public void Spawn(Vector2 pos, Vector2 target)
            {
                Position = pos;
                MoveTowards(target, 45);
            }

            public void SpawnFacing(Vector2 pos, Facing f)
            {
                Position = pos;
                velocity = FacingDirection(f) * 70;
            }

            public override void Update()
            {
                base.Update();

                if(touching != Touching.NONE)
                {
                    //Touched edge of screen
                    exists = false;
                }
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                if(other is Player p && p.state == PlayerState.GROUND)
                {
                    p.ReceiveDamage(1);
                }
                else if(other is Dust d)
                {
                    parent.ExplodeAt(d);
                    exists = false;
                }
                else if(other is Mist)
                {
                    exists = false;
                }
            }
        }

        public class Mist : Entity
        {
            public Mist() : base(Vector2.Zero,"briar_mist",24,24,Drawing.DrawOrder.FG_SPRITES)
            {
                AddAnimation("a", CreateAnimFrameArray(0, 1), 5);
                Play("a");
                opacity = 0.7f;
            }

            public void Spawn(Vector2 pos)
            {
                Position = pos;
                Flicker(1);
            }
        }
    }
}
