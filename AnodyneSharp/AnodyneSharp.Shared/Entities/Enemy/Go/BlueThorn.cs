using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnodyneSharp.Entities.Enemy.Go
{
    class BlueThorn : BigThorn
    {
        FireEye shooter = new();

        public BlueThorn(Vector2 pos) : base(pos, "briar_arm_right")
        {
            AddAnimation("off", CreateAnimFrameArray(1, 2, 3, 0), 4);
            Play("off");
            offset.X = 40;
            Position.X += offset.X;
        }

        public override IEnumerator GetAttacked(BigThorn blue, Player player)
        {
            float tm = 0;
            while (!MathUtilities.MoveTo(ref tm, 1.5f, 1)) yield return null;

            shooter.exists = true;
            shooter.Start(3 - Health, player);
            while (shooter.exists) yield return null;

            yield break;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { shooter };
        }

        public class FireEye : Entity
        {
            IEnumerator state;
            EntityPool<Fireball> fireballs = new(12, () => new());
            EntityPool<Dust> dust = new(4, () => new());

            public FireEye() : base(Vector2.Zero,"briar_fire_eye",16,16,Drawing.DrawOrder.BG_ENTITIES)
            {
                AddAnimation("shoot", CreateAnimFrameArray(6, 7, 8, 9), 10);
                AddAnimation("grow", CreateAnimFrameArray(0, 1, 2, 3, 4, 5), 10, false);
                AddAnimation("ungrow", CreateAnimFrameArray(5, 4, 3, 2, 1, 0), 10, false);
            }

            public void Start(int phase, Player p)
            {
                Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
                Play("grow");
                Position = tl + (phase == 0 ? new Vector2(70, 80) : new Vector2(50, 100));

                dust.Spawn((d) => { d.exists = true; d.Position = tl + new Vector2(1, 7) * 16; d.Play("unpoof"); });
                dust.Spawn((d) => { d.exists = true; d.Position = tl + new Vector2(8, 7) * 16; d.Play("unpoof"); });
                dust.Spawn((d) => { d.exists = true; d.Position = tl + new Vector2(3, 5) * 16; d.Play("unpoof"); });
                dust.Spawn((d) => { d.exists = true; d.Position = tl + new Vector2(1, 5) * 16; d.Play("unpoof"); });

                state = CoroutineUtils.OnceEvery(() => fireballs.Spawn((f) => f.Spawn(Position + Vector2.One * 4, p.Position)), 0.4f);
            }

            public override void Update()
            {
                base.Update();
                state?.MoveNext();
            }



            public override IEnumerable<Entity> SubEntities()
            {
                return fireballs.Entities.Concat(dust.Entities);
            }

        }

        [Collision(typeof(Player),typeof(Dust),KeepOnScreen = true)]
        public class Fireball : Entity
        {
            public Fireball() : base(Vector2.Zero, "briar_fire_eye", 16,16, Drawing.DrawOrder.ENTITIES)
            {
                AddAnimation("move", CreateAnimFrameArray(12, 13), 12);
                Play("move");
                width = height = 6;
                CenterOffset();
            }

            public void Spawn(Vector2 pos, Vector2 target)
            {
                Position = pos;
                MoveTowards(target, 45);
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
            }
        }
    }
}
