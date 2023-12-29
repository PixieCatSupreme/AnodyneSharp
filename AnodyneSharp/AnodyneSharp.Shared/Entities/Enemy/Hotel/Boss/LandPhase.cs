using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Enemy.Hotel.Boss
{
    [NamedEntity("Eye_Boss", null, 4), Collision(typeof(Player), typeof(Broom))]
    public class LandPhase : Entity
    {
        EnemyMarker death_marker = new();

        int health = 6;

        bool broom_hit = false;

        IEnumerator state;
        EntityPool<Bullet> bullets;
        EntityPool<BulletSplash> splashes;

        const bool TEST_LAND = false;

        Vector2 tl;

        Player player;
        EntityPreset preset;

        bool bossRush;

        public static AnimatedSpriteRenderer GetSprite() => new("eye_boss_water", 24, 24,
            new Anim("closed", new int[] { 3 }, 1),
            new Anim("walk", new int[] { 4, 5 }, 6),
            new Anim("blink_land", new int[] { 5, 6, 7, 6, 5 }, 6, false)
            );

        public LandPhase(EntityPreset preset, Player p) : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            //Water phase is needed to start the boss song, which means it at least spawned this time through the map, and it sets its Alive back to true until that phase is done
            bool water_phase_dead = !EntityManager.GetLinkGroup(preset.LinkID).Where(e => e != preset).First().Alive;
            if ((SoundManager.CurrentSongName != "hotel-boss" || !water_phase_dead || GlobalState.events.GetEvent("BossRushLandDefeated") == 1) && !TEST_LAND)
            {
                //Death marker being set to exists false does not increment this value here for some reason. Explicitly setting it makes the key spawn
                GlobalState.ENEMIES_KILLED = 1;
                death_marker.exists = false;
                exists = false;
                return;
            }
            (GlobalState.Map as MapData.Map).IgnoreMusicNextUpdate(); //Make sure music doesn't change back if player moves back up

            tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + Vector2.One * 16;

            this.preset = preset;
            player = p;
            p.grid_entrance = tl + new Vector2(70, 50);

            shadow = new(this, new Vector2(6, -8), ShadowType.Big);
            shadow.visible = false;

            splashes = new(18, () => new());
            bullets = new(8, () => new(splashes));

            if (preset.Activated)
            {
                Play("walk");
                Position = tl + new Vector2(16, 75);
                state = LandLogic(preset, p, tl);
            }
            else
            {
                state = ToLand(preset, p, tl);
                state.MoveNext(); //initial setup
            }

            bossRush = preset.TypeValue == "boss_rush";
        }

        IEnumerator ToLand(EntityPreset preset, Player p, Vector2 tl)
        {
            Play("closed");
            Solid = false; //No collision during this part
            Position = tl + new Vector2(44, -32);

            yield return null; //initial setup

            velocity.Y = 8;

            while (
                !(MathUtilities.MoveTo(ref y_push, 22, 40)
                & MathUtilities.MoveTo(ref opacity, 1, 1.8f))
                )
                yield return null;

            while (Position.Y < tl.Y)
                yield return null;

            velocity = Vector2.Zero;
            Play("walk");
            y_push = 0;
            SoundManager.PlaySoundEffect("bubble_triple");

            void shadowPos()
            {
                shadow.SetFrame(offset.Y switch
                {
                    <= 20 => 4,
                    <= 40 => 3,
                    _ => 2,
                });
            }
            shadow.visible = true;
            while (!MathUtilities.MoveTo(ref offset.Y, 90, 90))
            {
                shadowPos();
                yield return null;
            }
            Position = tl + new Vector2(16, 75);

            SoundManager.PlaySoundEffect("fall_1");

            while (!MathUtilities.MoveTo(ref offset.Y, 0, 180))
            {
                shadowPos();
                yield return null;
            }
            shadow.visible = false;

            GlobalState.screenShake.Shake(0.03f, 0.5f);

            preset.Activated = true; //Make sure that if player returns from quick phase 1 revisit boss is already on land
            state = LandLogic(preset, p, tl);
            yield break;
        }

        IEnumerator LandLogic(EntityPreset preset, Player p, Vector2 tl)
        {
            Solid = true;
            height = 11;
            offset.Y = sprite.Height - height;
            Position += offset;

            Vector2 base_pt = Position - Vector2.One * 16;

            var pace = Pace(base_pt);
            var blink = Blink();

            while (true)
            {
                while (!broom_hit)
                {
                    pace.MoveNext();
                    MathUtilities.MoveTo(ref Position.X, pace.Current.X, 60);
                    MathUtilities.MoveTo(ref Position.Y, pace.Current.Y, 60);
                    blink.MoveNext();
                    yield return null;
                }

                if (p.Position.X > Position.X)
                {
                    shadow.visible = true;
                    shadow.offset = new Vector2(4, 10);
                    Parabola_Thing parabola = new(this, 16, 0.6f);
                    velocity.X = -20;
                    while (!parabola.Tick())
                        yield return null;
                    velocity = Vector2.Zero;
                    SoundManager.PlaySoundEffect("wb_tap_ground");
                    shadow.visible = false;
                }

                while (!MathUtilities.MoveTo(ref Position.X, tl.X + 16 * 8 - width, 1.7f * 60) | !MathUtilities.MoveTo(ref Position.Y, tl.Y + 57, 60))
                    yield return null;

                GlobalState.screenShake.Shake(0.05f, 0.3f);
                SoundManager.PlaySoundEffect("hit_ground_1");
                ShootBullet(3);

                yield return null;
            }
        }

        IEnumerator<Vector2> Pace(Vector2 base_pt)
        {
            Vector2 pos = base_pt + Vector2.One * 16;
            while (true)
            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 1, 1))
                    yield return pos;
                if (GlobalState.RNG.NextDouble() < 0.7)
                {
                    pos = base_pt;
                    pos.X += GlobalState.RNG.Next(3) * 16;
                    pos.Y += GlobalState.RNG.Next(3) * 16;
                }
            }
        }

        IEnumerator Blink()
        {
            float[] times = new float[6] { 1, 1, 0.9f, 0.8f, 0.7f, 0.65f };
            while (true)
            {
                float t = 0;
                while (t < times[6 - health])
                {
                    t += GameTimes.DeltaTime;
                    if (CurAnimName == "blink_land" && AnimFinished)
                        Play("walk");
                    yield return null;
                }
                Play("blink_land");
                SoundManager.PlaySoundEffect("slasher_atk");
                ShootBullet();
            }
        }

        IEnumerator Die()
        {
            Solid = false;

            GlobalState.StartCutscene = DeathCutscene();

            SoundManager.StopSong();
            SoundManager.PlaySoundEffect("sun_guy_death_long");
            GlobalState.flash.Flash(1, Color.White);
            velocity = Vector2.Zero;

            if (!bossRush)
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("eyeboss", "after_fight");

                while (!GlobalState.LastDialogueFinished) yield return null;
            }

            offset = Vector2.One * 5;

            for (int i = 0; i < 10; ++i)
            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 0.3f, 1)) yield return null;
                Position += new Vector2(GlobalState.RNG.Next(-4, 5), GlobalState.RNG.Next(-4, 5));
                GlobalState.SpawnEntity(new Explosion(this));
                yield return null;
            }

            GlobalState.flash.Flash(2, Color.White, () => visible = false);
            SoundManager.PlaySoundEffect("sun_guy_death_long");

            shadow.exists = false;

            while (bullets.Alive > 0 || splashes.Alive > 0) yield return null;

            if (bossRush)
            {
                exists = false;
                SoundManager.PlaySong("bedroom");
                GlobalState.events.SetEvent("BossRushLandDefeated", 1);
            }
            else
            {
                preset.Alive = exists = false;
                GlobalState.events.BossDefeated.Add(GlobalState.CURRENT_MAP_NAME);
                SoundManager.PlaySong("hotel");
            }

            death_marker.exists = false;

            yield break;
        }

        IEnumerator<CutsceneEvent> DeathCutscene()
        {
            //Make sure player can move around but not be hit by remaining bullets
            while (exists)
            {
                player.dontMove = false;
                player.actions_disabled = false;
                yield return null;
            }
            yield break;
        }

        void ShootBullet(int amount = 1)
        {
            bullets.Spawn(b => b.Spawn(Position + new Vector2(width, 2), player.Position), amount);
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
            broom_hit = false;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (!Solid) return;

            if (other is Broom && !_flickering)
            {
                broom_hit = true;
                health--;
                SoundManager.PlaySoundEffect("broom_hit");
                Flicker(1f);
                if (health == 0)
                    state = Die();
            }
            else if (other is Player p)
            {
                p.ReceiveDamage(1);
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            if (exists)
                return new List<Entity>() { death_marker }.Concat(bullets.Entities).Concat(splashes.Entities);
            return Enumerable.Empty<Entity>();
        }

        [Collision(typeof(Player))]
        private class Bullet : Entity
        {

            Parabola_Thing parabola;
            const float duration = 1.5f;
            EntityPool<BulletSplash> splashes;

            public static AnimatedSpriteRenderer GetSprite() => new("eye_boss_bullet", 16, 16,
                new Anim("move", new int[] { 6, 7 }, 12),
                new Anim("pop", new int[] { 2, 3, 4, 5 }, 24, false)
                );

            public Bullet(EntityPool<BulletSplash> s) : base(Vector2.Zero, GetSprite(), Drawing.DrawOrder.FG_SPRITES)
            {
                width = height = 8;
                CenterOffset();

                splashes = s;

                shadow = new(this, Vector2.UnitX * 2);

                parabola = new(this, 45, duration);
            }

            public void Spawn(Vector2 pos, Vector2 target)
            {
                Play("move");
                Position = pos;

                velocity = (target - pos) / duration;
                velocity += new Vector2(GlobalState.RNG.NextSingle() * 5, GlobalState.RNG.NextSingle() * 5);
                parabola.ResetTime();
            }

            public override void Update()
            {
                base.Update();
                if (CurAnimName == "move")
                {
                    if (parabola.Tick())
                    {
                        Play("pop");
                        velocity = Vector2.Zero;
                    }
                }
                else
                {
                    if (AnimFinished)
                    {
                        exists = false;
                        SoundManager.PlaySoundEffect("4sht_shoot");
                        splashes.Spawn(s => s.Spawn(Position), 4);
                    }
                }
            }

            public override void Collided(Entity other)
            {
                if (offset.Y < 7)
                    ((Player)other).ReceiveDamage(1);
            }
        }

        [Collision(typeof(Player))]
        private class BulletSplash : Entity
        {
            Parabola_Thing parabola;

            public static AnimatedSpriteRenderer GetSprite() => new("eye_boss_splash", 8, 8,
                new Anim("move", new int[] { 0, 1 }, 10),
                new Anim("pop", new int[] { 2, 3 }, 12, false)
                );

            public BulletSplash() : base(Vector2.Zero, GetSprite(), Drawing.DrawOrder.FG_SPRITES)
            {
                shadow = new(this, Vector2.Zero);

                parabola = new(this, 36, 1 + 0.4f * GlobalState.RNG.NextSingle());
            }

            public void Spawn(Vector2 pos)
            {
                Position = pos;
                Play("move");
                parabola.ResetTime();
                velocity.X = GlobalState.RNG.Next(-20, 21);
                velocity.Y = MathF.Sqrt(20 * 20 - velocity.X * velocity.X);
                if (GlobalState.RNG.NextSingle() < 0.5f) velocity.Y *= -1;
            }

            public override void Update()
            {
                base.Update();
                if (CurAnimName == "move" && parabola.Tick())
                {
                    Play("pop");
                }
                else
                {
                    if (AnimFinished)
                    {
                        exists = false;
                        SoundManager.PlaySoundEffect("4sht_pop");
                    }
                }
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                Player p = (Player)other;
                if (offset.Y < 9 && !p.invincible)
                {
                    p.slowMul = 0.3f;
                    p.slowTicks = 100;
                    SoundManager.PlaySoundEffect("bubble_1", "bubble_1", "bubble_2", "bubble_3");
                }
            }
        }
    }
}
