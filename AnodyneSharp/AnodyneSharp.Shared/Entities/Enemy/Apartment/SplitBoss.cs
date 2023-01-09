using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.GameEvents;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Apartment
{
    [Collision(typeof(Player))]
    class BaseSplitBoss : Entity
    {
        public bool damage_player = false;

        public static AnimatedSpriteRenderer GetSprite() => new("splitboss", 24, 32,
            new Anim("float", new int[] { 0, 1, 2, 1 }, 5),
            new Anim("idle_r", new int[] { 4, 5, 6, 5 }, 5),
            new Anim("dash_r", new int[] { 10, 11 }, 8),
            new Anim("dash_d", new int[] { 8, 9 }, 8),
            new Anim("die", new int[] { 0, 3 }, 14)
            );

        public BaseSplitBoss(Vector2 position) : base(position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            width = 16;
            height = 20;
            CenterOffset();

            Solid = false;
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (other is Player p && damage_player)
            {
                p.ReceiveDamage(1);
            }
        }

        protected override void AnimationChanged(string name)
        {
            base.AnimationChanged(name);
            if (name.EndsWith("_r"))
            {
                offset.Y = 6;
            }
            else
            {
                CenterOffset(false);
            }
        }
    }

    [NamedEntity("Splitboss"), Enemy, Collision(typeof(Broom)), Events(typeof(StartScreenTransition), typeof(StartWarp))]
    class SplitBoss : BaseSplitBoss
    {
        Player player;
        EntityPreset _preset;

        EntityPool<Bullet> bullets = new(12, () => new());

        Copy[] copies = new Copy[2]
        {
            new(true),
            new(false)
        };

        IEnumerator state;

        //top left of main fight area
        Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(32, 48);

        bool hit_this_frame = false;
        bool vulnerable = false;
        float invuln_time = 0f;

        int health = 13;

        int Phase => health switch
        {
            > 8 => 0,
            > 4 => 1,
            _ => 2
        };

        public SplitBoss(EntityPreset preset, Player p) : base(preset.Position)
        {
            _preset = preset;
            player = p;

            Position = tl + new Vector2(48 - width / 2, 32);

            p.grid_entrance = tl + new Vector2(42, 40);

            state = Intro();
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
            MathUtilities.MoveTo(ref invuln_time, 0, 1);
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            hit_this_frame = false;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (other is Broom)
            {
                hit_this_frame = true;
                if (vulnerable && invuln_time == 0)
                {
                    invuln_time = 0.8f;
                    Flicker(invuln_time);
                    SoundManager.PlaySoundEffect("sb_hurt");
                    SoundManager.PlaySoundEffect("broom_hit");
                    health--;
                    if (health == 0)
                    {
                        vulnerable = false;
                        damage_player = false;
                        state = Die();
                    }
                }
            }
        }

        IEnumerator Intro()
        {
            VolumeEvent volume = new(0, 0.3f);

            GlobalState.SpawnEntity(volume);

            while (!hit_this_frame)
            {
                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("splitboss", "before_fight");

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            volume.exists = false;

            SoundManager.PlaySoundEffect("sb_split");
            SoundManager.PlaySong("apartment-boss");

            visible = false;
            foreach (Copy c in copies)
            {
                c.exists = true;
                c.Position = Position;
                c.velocity.X = 50;
            }
            copies[0].velocity.X *= -1;

            while (!MathUtilities.MoveTo(ref copies[0].opacity, 0, 0.6f))
            {
                copies[1].opacity = copies[0].opacity;
                yield return null;
            }

            opacity = 0;
            visible = true;
            Position.Y = tl.Y - 32;
            copies[0].Position.X = Position.X - 40;
            copies[1].Position.X = Position.X + 40;
            foreach (Copy c in copies)
            {
                c.velocity = Vector2.Zero;
                c.Position.Y = Position.Y;
                c.Play("float", true); //sync up float animation
            }
            Play("float", true);

            while (!MathUtilities.MoveTo(ref opacity, 1, 0.6f))
            {
                copies[0].opacity = copies[1].opacity = opacity;
                yield return null;
            }

            while (!MathUtilities.MoveTo(ref copies[0].Position.X, Position.X, 30) | !MathUtilities.MoveTo(ref copies[1].Position.X, Position.X, 30))
            {
                Flicker(0.2f);
                copies[0].Flicker(0.2f); copies[1].Flicker(0.2f);
                yield return null;
            }

            copies[0].exists = copies[1].exists = false;

            state = Shoot();

            vulnerable = true;
            damage_player = true;

            yield break;
        }

        IEnumerator Die()
        {
            velocity = Vector2.Zero;
            opacity = 1f;
            damage_player = false;
            vulnerable = false;

            foreach (Bullet b in bullets.Entities)
            {
                b.Deactivate();
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("splitboss", "after_fight");

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            SoundManager.PlaySoundEffect("big_door_locked");

            float tm = 0;
            float tm_max = 5;
            GlobalState.gameScreenFade.fadeColor = Color.White;

            Play("die");
            Flicker(tm_max);

            while (tm < tm_max)
            {
                tm += GameTimes.DeltaTime;
                SoundManager.SetSongVolume(SoundManager.GetVolume() - GameTimes.DeltaTime * 0.3f);
                GlobalState.gameScreenFade.ForceAlpha(tm / tm_max);
                int r = (int)(65 * tm / tm_max);
                Position = tl + 40 * Vector2.One + new Vector2(GlobalState.RNG.Next(-r, r), GlobalState.RNG.Next(-r, r));
                foreach (Entity e in bullets.Entities.Concat(copies))
                {
                    MathUtilities.MoveTo(ref e.opacity, 0, 6);
                }
                yield return null;
            }

            SoundManager.PlaySong("apartment");
            _preset.Alive = exists = false;
            GlobalState.events.BossDefeated.Add(GlobalState.CURRENT_MAP_NAME);

            GlobalState.SpawnEntity(new FadeOutGameScreenFade());

            yield break;
        }

        IEnumerator Dash()
        {
            int num_dashes = Phase + 2;
            int num_drops = Phase + 2;

            List<Bullet> spawned = new();

            if (MapUtilities.GetRoomCoordinate(Position) != GlobalState.CurrentMapGrid)
            {
                //If done right after another dash, instantly start appearing on the left side of the screen
                opacity = 0;
            }

            //Dash and place fires
            for (int dash = 0; dash < num_dashes; ++dash)
            {
                Play("idle_r");

                while (!MathUtilities.MoveTo(ref opacity, 0, 6))
                    yield return null;

                Position.X = tl.X - 32;

                while (!MathUtilities.MoveTo(ref opacity, 1, 9))
                {
                    Position.Y = player.Position.Y;
                    yield return null;
                }

                float dash_timer = 0.5f;
                while (dash_timer > 0)
                {
                    Position.Y = player.Position.Y;
                    dash_timer -= GameTimes.DeltaTime;
                    yield return null;
                }

                Play("dash_r");
                SoundManager.PlaySoundEffect("sb_dash");

                velocity = Vector2.UnitX * (80 + 20 * Phase);
                float next_drop = tl.X + GlobalState.RNG.Next(16);

                bool CheckDashEnd()
                {
                    if (Position.X < tl.X + 16 * 8) return false;
                    return MathUtilities.MoveTo(ref opacity, 0, 6);
                }

                int max = num_drops;

                while (!CheckDashEnd())
                {
                    offset.Y = player.offset.Y + 2;

                    if (max > 0 && Position.X > next_drop && Position.X < tl.X + 6 * 16 && bullets.Entities.Count() > spawned.Count)
                    {
                        max--;
                        bullets.Spawn(t => { t.Spawn(Position, false); t.Flicker(0.05f); spawned.Add(t); });
                        SoundManager.PlaySoundEffect("sb_ball_appear");
                        next_drop = Position.X + GlobalState.RNG.Next(16, 32);
                    }
                    yield return null;
                }
                velocity = Vector2.Zero;
            }


            //unleash them
            foreach (Bullet b in spawned)
            {
                b.Flicker(0);
            }
            float timer = 0.5f;
            while (timer > 0)
            {
                timer -= GameTimes.DeltaTime;
                yield return null;
            }

            SoundManager.PlaySoundEffect("sb_split");

            foreach (Bullet b in spawned)
            {
                Vector2 target = player.Position + new Vector2(GlobalState.RNG.Next(-10, 10), GlobalState.RNG.Next(-10, 10));
                b.Activate((80 + Phase * 20) * Vector2.Normalize(target - b.Position));
                b.opacity = 0.99f;
            }

            while (bullets.Alive > 0)
            {
                yield return null;
            }

            opacity = 1f;

            ChooseNextAttack(0.3, 0.3);

            yield break;
        }

        IEnumerator Shoot()
        {
            velocity = Vector2.Zero;
            Position = tl + new Vector2(40, -32);
            Play("float");

            int num_rounds = new[] { 4, 10, 7 }[Phase];
            int num_bullets_per_round = new[] { 2, 1, 4 }[Phase];
            float wait_time = new[] { 0.7f, 0.2f, 0.8f }[Phase];

            for (int round = 0; round < num_rounds; ++round)
            {
                SoundManager.PlaySoundEffect("sb_ball_appear");

                int start = GlobalState.RNG.Next(0, 6);
                List<Bullet> spawned = new();
                bullets.Spawn(b =>
                {
                    b.Spawn(tl + new Vector2((16 - b.width) / 2 + (start % 6) * 16, -b.height), true);
                    start++;
                    spawned.Add(b);
                }, num_bullets_per_round);

                float t = 0;
                while (t < wait_time)
                {
                    t += GameTimes.DeltaTime;
                    yield return null;
                }

                foreach (Bullet b in spawned)
                {
                    b.Activate(Vector2.UnitY * 40);
                }
            }

            while (bullets.Alive != 0)
            {
                yield return null;
            }

            ChooseNextAttack(0.37, 0.38);

            yield break;
        }

        IEnumerator Split()
        {
            visible = true;
            vulnerable = false;
            damage_player = false;

            Play("float");

            Position = tl + new Vector2(40, -16);

            Flicker(1.5f);

            while (!hit_this_frame)
            {
                yield return null;
            }

            damage_player = true;

            opacity = copies[0].opacity = copies[1].opacity = 1f;
            copies[0].exists = copies[1].exists = true;
            copies[0].Play("idle_r"); copies[1].Play("idle_r");
            copies[0].Position = copies[1].Position = Position;

            velocity.Y = -40;
            copies[0].velocity.X = -40;
            copies[1].velocity.X = 40;

            while (!MathUtilities.MoveTo(ref opacity, 0, 1.2f))
            {
                copies[0].opacity = copies[1].opacity = opacity;
                yield return null;
            }

            velocity = copies[0].velocity = copies[1].velocity = Vector2.Zero;
            opacity = copies[0].opacity = copies[1].opacity = 0.7f;

            float tm = 0f;

            while (tm < 1f)
            {
                tm += GameTimes.DeltaTime;
                Position = player.Position - Vector2.UnitY * 24;
                copies[0].Position = player.Position + Vector2.UnitX * 25;
                copies[1].Position = player.Position - Vector2.UnitX * 25;
                yield return null;
            }

            //Warning for copies
            copies[0].Flicker(0.7f);
            copies[1].Flicker(0.7f);
            while (copies[0]._flickering)
            {
                Position = player.Position - Vector2.UnitY * 24;
                yield return null;
            }

            copies[0].damage_player = copies[1].damage_player = true;

            float split_vel = 40 + 10 * Phase;

            copies[0].velocity.X = -split_vel;
            copies[1].velocity.X = split_vel;
            SoundManager.PlaySoundEffect("sb_dash");

            while (!MathUtilities.MoveTo(ref copies[0].opacity, 0, 1.2f) | !MathUtilities.MoveTo(ref copies[1].opacity, 0, 1.2f))
            {
                Position = player.Position - Vector2.UnitY * 24;
                yield return null;
            }

            copies[0].exists = copies[1].exists = copies[0].damage_player = copies[1].damage_player = false;

            Flicker(1);

            while (_flickering)
            {
                yield return null;
            }

            vulnerable = true;

            velocity.Y = split_vel;

            while (!MathUtilities.MoveTo(ref opacity, 0, 1.2f))
            {
                yield return null;
            }

            opacity = 1f;
            velocity = Vector2.Zero;

            if (Phase <= 1)
            {
                ChooseNextAttack(0.3, 0.3);
            }
            else
            {
                ChooseNextAttack(0.5, 0.25);
            }


            yield break;
        }

        void ChooseNextAttack(double dash_chance, double split_chance)
        {
            double r = GlobalState.RNG.NextDouble();
            if (r < dash_chance)
            {
                state = Dash();
            }
            else if (r < dash_chance + split_chance)
            {
                state = Split();
            }
            else
            {
                state = Shoot();
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return bullets.Entities.Concat(copies);
        }

        [Events(typeof(StartWarp), typeof(StartScreenTransition))]
        class FadeOutGameScreenFade : Entity
        {
            public FadeOutGameScreenFade() : base(Vector2.Zero)
            {
                visible = false;
            }

            public override void Update()
            {
                base.Update();
                GlobalState.gameScreenFade.ChangeAlpha(-3);
            }

            public override void OnEvent(GameEvent e)
            {
                base.OnEvent(e);
                GlobalState.gameScreenFade.Deactivate();
            }
        }

        [Collision(typeof(Player))]
        class Bullet : Entity
        {
            bool active = false;

            public Bullet() : base(Vector2.Zero, new AnimatedSpriteRenderer("splitboss_fireball", 16, 16,
                new Anim("pulsate", new int[] { 0, 1, 2, 3 }, 12), new Anim("fizzle", new int[] { 4, 5, 6, 7 }, 12, false)), Drawing.DrawOrder.BG_ENTITIES)
            {
                width = height = 6;
                CenterOffset();
            }

            public override void Update()
            {
                base.Update();
                if (AnimFinished || opacity == 0)
                {
                    exists = false;
                }

                if (_flickering)
                {
                    Flicker(0.05f);
                }

                if (opacity != 1f)
                {
                    MathUtilities.MoveTo(ref opacity, 0, 0.3f);
                }

                if (opacity == 1f && MapUtilities.GetInGridPosition(Position).Y > 8 * 16)
                {
                    Play("fizzle");
                }
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                if (other is Player p && p.state != PlayerState.AIR && active)
                {
                    p.ReceiveDamage(1);
                }
            }

            public void Deactivate()
            {
                active = false;
            }

            public void Activate(Vector2 vel)
            {
                active = true;
                velocity = vel;
            }

            public void Spawn(Vector2 pos, bool startsHurtingPlayer)
            {
                Position = pos;
                Play("pulsate");
                velocity = Vector2.Zero;
                active = startsHurtingPlayer;
                opacity = 1f;
            }
        }
    }

    class Copy : BaseSplitBoss
    {
        public Copy(bool right) : base(Vector2.Zero)
        {
            if (right)
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            }
            exists = false;
        }
    }
}
