using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Go
{
    [Collision(typeof(Player))]
    public class BriarBossBody : Entity
    {
        EntityPool<Bullet> bullets = new(16, () => new());
        EntityPool<Thorn> thorns = new(8, () => new());

        public IEnumerator State;
        Player player;

        public BriarBossBody(Vector2 pos, Player player) : base(pos, "briar_body", 128, 80, Drawing.DrawOrder.ENTITIES)
        {
            height = 60;
            this.player = player;
        }

        public override void Update()
        {
            base.Update();
            if (!State?.MoveNext() ?? false)
            {
                State = null;
            }
        }

        public IEnumerator Attack(int phase)
        {
            var thorn_attack = ThornAttack(phase);
            var bullet_attack = CoroutineUtils.OnceEvery(Bullets(phase), 1.4f - 0.2f * phase / 2);

            while (thorn_attack.MoveNext())
            {
                bullet_attack.MoveNext();
                yield return null;
            }

            yield break;
        }

        IEnumerator<string> Bullets(int phase)
        {
            int[] base_xpos = { 36, 52, 69, 84 };
            Vector2 extra_left = new(-10, 45);
            Vector2 extra_right = new(10, 45);
            Vector2 acc_left = new(5, 20);
            Vector2 acc_right = new(-5, 20);
            while (true)
            {
                foreach (int x in base_xpos)
                {
                    bullets.Spawn(b => b.Spawn(Position + new Vector2(x, 60), Vector2.UnitY * 40, Vector2.UnitY * 25));
                }
                if (phase >= 2)
                {
                    bullets.Spawn(b => b.Spawn(Position + new Vector2(20, 44), extra_left, acc_left));
                    bullets.Spawn(b => b.Spawn(Position + new Vector2(101, 44), extra_right, acc_right));
                }
                if (phase >= 4)
                {
                    bullets.Spawn(b => b.Spawn(Position + new Vector2(4, 44), extra_left, acc_left));
                    bullets.Spawn(b => b.Spawn(Position + new Vector2(116, 44), extra_right, acc_right));
                }
                yield return null;
            }
        }

        IEnumerator ThornAttack(int phase)
        {
            float[] times = { 0.18f, 0.16f, 0.14f, 0.13f, 0.11f, 0.09f };
            int[] number_attacks = { 2, 2, 2, 3, 3, 4 };

            for (int i = 0; i < number_attacks[phase]; ++i)
            {
                var spawn_thorns = CoroutineUtils.OnceEvery(Thorns(phase), times[phase]);
                while (spawn_thorns.MoveNext()) yield return null;
                while (thorns.Alive != 0) yield return null;
            }
        }

        IEnumerator<string> Thorns(int phase)
        {
            SoundManager.PlaySoundEffect("briar_shine");
            for (int i = 0; i < 8; ++i)
            {
                int player_pos = (int)(player.Position.Y - Position.Y) / 16;
                player_pos = Math.Clamp(player_pos, (i > 1 && i < 6) ? 4 : 5, 8);

                thorns.Spawn(t => t.Spawn(Position + new Vector2(i, player_pos) * 16, phase));
                yield return null;
            }
            yield break;
        }

        public override void Collided(Entity other)
        {
            other.Position.Y = Hitbox.Bottom;
            (other as Player).ReceiveDamage(1);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return bullets.Entities.Concat(thorns.Entities);
        }

        [Collision(typeof(Player))]
        class Thorn : Entity
        {
            const int harmful_frame = 9;
            public Thorn() : base(Vector2.Zero, "briar_ground_thorn", 16, 16, Drawing.DrawOrder.ENTITIES)
            {
                width = height = 6;
                offset = new(5, 4);
                AddAnimation("attack", CreateAnimFrameArray(0, 1, 2, 0, 3, 3, 3, 6, 7, 8, 9, 10, 9, 10, 9, 10, 8, 7, 6), 8, false);
            }

            public void Spawn(Vector2 pos, int phase)
            {
                Position = pos + Vector2.UnitX * offset.X;
                Play("attack", newFramerate: 12 + phase);
            }

            public override void Update()
            {
                base.Update();
                if (AnimFinished)
                    exists = false;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                Player p = other as Player;
                if (p.state == PlayerState.GROUND && Frame >= harmful_frame)
                    p.ReceiveDamage(1);
            }
        }

        [Collision(typeof(Player))]
        class Bullet : Entity
        {
            public Bullet() : base(Vector2.Zero, "briar_thorn_bullet", 16, 16, Drawing.DrawOrder.ENTITIES)
            {
                width = height = 6;
                offset = Vector2.One * 5;
                AddAnimation("move", CreateAnimFrameArray(0, 1), 10);
                Play("move");
            }

            public void Spawn(Vector2 pos, Vector2 vel, Vector2 acc)
            {
                Position = pos;
                velocity = vel;
                acceleration = acc;
            }

            public override void Update()
            {
                base.Update();
                if (MapUtilities.GetInGridPosition(Position).Y > 150)
                    exists = false;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                Player p = other as Player;
                if (p.state == PlayerState.GROUND)
                    p.ReceiveDamage(1);
            }
        }
    }
}
