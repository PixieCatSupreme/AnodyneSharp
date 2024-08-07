﻿using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Redcave
{
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom))]
    public class Red_Boss : Entity
    {
        IEnumerator state;
        Player player;
        EntityPreset preset;
        Ripple ripple;

        EntityPool<SplashBullet> splash_bullets;
        EntityPool<Tentacle> tentacles;
        SmallWave small_wave;
        BigWave big_wave;

        ProximitySensor[] sensors;
        public Touching proximity_hits = Touching.NONE;

        int health = 12;
        float invincible_timer = 0f;

        const float push_tick_max = 0.15f;

        float amp = 0;
        float pushdown_timer = 0f;

        bool loopSFX = false;

        bool bossRush;

        public const string DamageDealer = "Red Boss";
        public const string TentacleDamageDealer = "Red Boss tentacle";
        public const string BulletDamageDealer = "Red Boss bullet";

        class SplashState : TimerState
        {
            public int got_too_close = 0;
            public SplashState()
            {
                AddTimer(3f, "Splash");
                AddTimer(3f, "Tentacles");
            }
        }

        class DashState : TimerState
        {
            public int got_too_close = 0;
            public DashState()
            {
                AddTimer(1.5f, "Tentacles");
                AddTimer(5f, "EndDash");
            }
        }

        class StunState : AbstractState
        {
            public IEnumerator stateLogic;
        }

        public static AnimatedSpriteRenderer GetSprite() => new("red_boss", 32, 32,
            new Anim("close_eyes", new int[] { 1 }, 1),
            new Anim("bob", new int[] { 0 }, 1),
            new Anim("warn", new int[] { 2 }, 1),
            new Anim("die", new int[] { 0, 1, 2, 1 }, 3, false)
            );

        public Red_Boss(EntityPreset preset, Player p) : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            height = 19;
            width = 26;
            offset = new Vector2(3, 13);

            ripple = new(this);
            Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
            Point splash_start = new(2, 2);
            splash_bullets = new(4, () =>
            {
                splash_start.Y++;
                return new(tl + splash_start.ToVector2() * 16 + Vector2.One * 3);
            });

            int start = 0;
            tentacles = new(4, () => new(start++));

            sensors = new ProximitySensor[]
            {
                new(Touching.LEFT,this),
                new(Touching.RIGHT,this),
                new(Touching.UP,this),
                new(Touching.DOWN,this),
            };

            small_wave = new(this);
            big_wave = new(this);

            player = p;
            this.preset = preset;

            state = State();

            bossRush = preset.TypeValue == "boss_rush";
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { ripple, small_wave, big_wave }.Concat(splash_bullets.Entities).Concat(sensors).Concat(tentacles.Entities);
        }

        public override void Update()
        {
            base.Update();
            invincible_timer -= GameTimes.DeltaTime;
            state.MoveNext();
            proximity_hits = Touching.NONE;

            if (loopSFX)
            {
                SoundManager.PlaySoundEffect("bubble_loop"); //Call each frame to get looping behavior out of a sound effect
            }
        }

        public override void Collided(Entity other)
        {
            if (health == 0) return;

            if (other is Player p)
            {
                p.ReceiveDamage(1, DamageDealer);
            }
            else if (other is Broom && invincible_timer < 0)
            {
                Flicker(0.5f);
                invincible_timer = 1.3f;
                health--;
                SoundManager.PlaySoundEffect("redboss_moan");
            }
        }

        IEnumerator State()
        {
            float push_timer = 0f;

            y_push = sprite.Height;
            player.grid_entrance = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + Vector2.One * 20;

            GlobalState.SpawnEntity(new VolumeEvent(0, 3));

            while (MapUtilities.GetInGridPosition(player.Position).X < 48)
            {
                yield return null;
            }

            loopSFX = true;

            if (!bossRush)
            {
                GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("redboss", "before_fight");


                while (!GlobalState.LastDialogueFinished)
                {
                    push_timer += GameTimes.DeltaTime;
                    if (push_timer >= push_tick_max)
                    {
                        push_timer = 0f;
                        if (y_push > 0)
                        {
                            GlobalState.screenShake.Shake(0.021f, 0.1f);
                            y_push--;
                        }
                    }
                    yield return null;
                }

            }
            else
            {
                while (y_push > 0)
                {
                    push_timer += GameTimes.DeltaTime;
                    if (push_timer >= push_tick_max)
                    {
                        push_timer = 0f;
                        if (y_push > 0)
                        {
                            GlobalState.screenShake.Shake(0.021f, 0.1f);
                            y_push--;
                        }
                    }
                    yield return null;
                }
            }

            loopSFX = false;

            SoundManager.PlaySong("redcave-boss");
            Play("bob");

            IState state = new StateMachineBuilder()
                .State<SplashState>("Splash")
                    .Enter((s) => amp = 5)
                    .Event("Splash", (s) =>
                     {
                         splash_bullets.Spawn(b => b.Spawn(), 4);
                     })
                    .Event("Tentacles", (s) =>
                    {
                        SpawnTentacles();
                        if (proximity_hits != Touching.NONE)
                        {
                            s.got_too_close++;
                            if (s.got_too_close == 2)
                            {
                                s.got_too_close = 0;
                                s.Parent.ChangeState("Stun");
                            }
                        }
                    })
                .End()
                .State<DashState>("Dash")
                    .Enter((s) =>
                    {
                        amp = 0;
                        velocity = new Vector2(30, 20);
                        Play("bob");
                    })
                    .Update((s, _) =>
                       {
                           Drawing.Effects.ScreenShake.Directions dirs = new();
                           Vector2 tl = MapUtilities.GetInGridPosition(Position);
                           Vector2 br = MapUtilities.GetInGridPosition(Position + new Vector2(width, height));
                           if (tl.Y < 2 * 16)
                           {
                               velocity.Y = 60;
                               dirs |= Drawing.Effects.ScreenShake.Directions.Vertical;
                           }
                           else if (br.Y > 16 * 8)
                           {
                               velocity.Y = -60;
                               dirs |= Drawing.Effects.ScreenShake.Directions.Vertical;
                           }

                           if (tl.X < 2 * 16)
                           {
                               velocity.X = 60;
                               dirs |= Drawing.Effects.ScreenShake.Directions.Horizontal;
                           }
                           else if (br.X > 16 * 8)
                           {
                               velocity.X = -60;
                               dirs |= Drawing.Effects.ScreenShake.Directions.Horizontal;
                           }
                           GlobalState.screenShake.Shake(0.05f, 0.1f, dirs);
                       })
                    .Event("Tentacles", (s) =>
                     {
                         SpawnTentacles();
                         if (proximity_hits != Touching.NONE)
                         {
                             s.got_too_close++;
                             if (s.got_too_close == 2)
                             {
                                 s.got_too_close = 0;
                                 s.Parent.ChangeState("Splash");
                             }
                         }
                     })
                    .Event("EndDash", (s) =>
                    {
                        s.Parent.ChangeState("Stun");
                    })
                    .Exit((s) => velocity = Vector2.Zero)
                .End()
                .State<StunState>("Stun")
                    .Enter((s) => s.stateLogic = StunStateLogic())
                    .Update((s, _) =>
                    {
                        if (!s.stateLogic.MoveNext())
                        {
                            s.Parent.ChangeState("Dash");
                        }
                    })
                .End()
                .Build();

            state.ChangeState("Splash");
            state.TriggerEvent("Splash"); //First time instantly fires splash bullets

            while (health > 0)
            {
                state.Update(GameTimes.DeltaTime);

                pushdown_timer += GameTimes.DeltaTime * 3;
                y_push = (int)(amp + MathF.Sin(pushdown_timer) * amp);
                yield return null;
            }

            velocity = Vector2.Zero;

            SoundManager.StopSong();

            if (!bossRush)
            {
                GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("redboss", "after_fight");
            }

            GlobalState.screenShake.Shake(0.05f, 0.1f);
            GlobalState.flash.Flash(1f, Color.Red);

            small_wave.exists = false;

            foreach (Entity bullet in splash_bullets.Entities)
            {
                bullet.exists = false;
            }
            foreach (Tentacle tentacle in tentacles.Entities)
            {
                tentacle.Despawn();
            }

            small_wave.exists = false;
            big_wave.exists = false;

            if (!bossRush)
            {
                while (!GlobalState.LastDialogueFinished)
                {
                    yield return null;
                }
            }

            Play("die");
            SoundManager.PlaySoundEffect("redboss_death");
            GlobalState.wave.active = true;

            y_push = 0;

            while (y_push < sprite.Height)
            {
                MathUtilities.MoveTo(ref ripple.opacity, 0, 0.3f);

                push_timer += GameTimes.DeltaTime;
                if (push_timer >= push_tick_max)
                {
                    push_timer = 0f;
                    y_push++;
                }
                yield return null;
            }

            float final_timer = 2f;
            while (final_timer > 0f)
            {
                final_timer -= GameTimes.DeltaTime;
                yield return null;
            }

            preset.Alive = exists = false;
            GlobalState.wave.active = false;


            if (!bossRush)
            {
                GlobalState.events.BossDefeated.Add("REDCAVE");
                SoundManager.PlaySong("redcave");
            }
            else
            {
                SoundManager.PlaySong("bedroom");
            }

            yield break;
        }

        private void SpawnTentacles()
        {
            if (tentacles.Alive == 0)
            {
                tentacles.Spawn((t) => t.Spawn(proximity_hits, this), 4);
            }
        }

        IEnumerator StunStateLogic()
        {
            Play("warn");

            amp = 5;

            Vector2 target = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(6, 4) * 16;

            small_wave.Rise();

            while (!(MathUtilities.MoveTo(ref Position.X, target.X, 30) & MathUtilities.MoveTo(ref Position.Y, target.Y, 30)))
                yield return null;

            while (y_push != 0)
                yield return null;

            loopSFX = true;
            amp = 13;

            while (y_push < amp)
                yield return null;

            small_wave.Launch();

            while (y_push != 0)
                yield return null;

            amp = 20;

            while (y_push < 18)
                yield return null;

            big_wave.Rise();

            while (big_wave.velocity == Vector2.Zero)
                yield return null;

            Play("close_eyes");

            while (y_push > 2)
                yield return null;

            amp = 5;

            while (small_wave.exists || big_wave.exists)
                yield return null;

            target -= Vector2.UnitX * 16;

            while (!(MathUtilities.MoveTo(ref Position.X, target.X, 30) & MathUtilities.MoveTo(ref Position.Y, target.Y, 30)))
                yield return null;

            loopSFX = false;

            yield break;
        }
    }

    [Collision(typeof(Player))]
    public class SmallWave : Entity
    {
        Vector2 spawn_point;

        public static AnimatedSpriteRenderer GetSprite() => new("red_boss_small_wave", 16, 64,
            new Anim("move", new int[] { 0, 1 }, 8),
            new Anim("rise", new int[] { 2, 3 }, 8),
            new Anim("fall", new int[] { 1, 2, 3, 4 }, 8, false)
            );

        public SmallWave(Red_Boss parent) : base(Vector2.Zero, GetSprite(), Drawing.DrawOrder.BG_ENTITIES)
        {
            spawn_point = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(96, 48);

            exists = false;
            immovable = true;
        }

        public void Rise()
        {
            Position = spawn_point;
            Play("rise");
            exists = true;
        }

        public void Launch()
        {
            velocity.X = -20;
            GlobalState.screenShake.Shake(0.03f, 1f);
            Play("move");
            SoundManager.PlaySoundEffect("small_wave");
        }

        public override void Collided(Entity other)
        {
            MathUtilities.MoveTo(ref other.Position.X, 0, 30);
        }

        public override void Update()
        {
            base.Update();
            if (AnimFinished)
            {
                velocity = Vector2.Zero;
                exists = false;
                return;
            }

            if (MapUtilities.GetInGridPosition(Position).X < 32)
            {
                Play("fall");
            }
        }
    }

    [Collision(typeof(Player))]
    public class BigWave : Entity
    {
        Vector2 spawn_point;
        bool disable_player_hit = true;

        public static AnimatedSpriteRenderer GetSprite() => new("red_boss_big_wave", 32, 80,
            new Anim("move", new int[] { 0, 1 }, 8),
            new Anim("rise", new int[] { 2, 1, 0 }, 8, false),
            new Anim("fall", new int[] { 1, 2, 3 }, 8, false)
            );

        public BigWave(Red_Boss parent) : base(Vector2.Zero, GetSprite(), Drawing.DrawOrder.BG_ENTITIES)
        {
            spawn_point = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(80, 48);

            exists = false;
            immovable = true;
        }

        public void Rise()
        {
            Position = spawn_point;
            Play("rise");
            exists = true;
        }

        public void Launch()
        {
            velocity.X = -40;
            GlobalState.screenShake.Shake(0.05f, 1f);
            Play("move");
            SoundManager.PlaySoundEffect("big_wave");
            disable_player_hit = false;
        }

        public override void Collided(Entity other)
        {
            if (other is Player p && !disable_player_hit)
            {
                SoundManager.PlaySoundEffect("player_hit_1");
                Vector2 targetInGrid = new(16, 8 * 16);
                Vector2 target = p.Position + (targetInGrid - MapUtilities.GetInGridPosition(p.Position));
                p.AutoJump(1, target, 50, -500);
                disable_player_hit = true;
            }
        }

        public override void Update()
        {
            base.Update();
            if (AnimFinished)
            {
                if (CurAnimName == "fall")
                {
                    velocity = Vector2.Zero;
                    exists = false;
                    return;
                }
                else //rise
                {
                    Launch();
                }
            }

            if (MapUtilities.GetInGridPosition(Position).X < 32)
            {
                Play("fall");
                disable_player_hit = true;
            }
        }
    }

    //Actually the tentacle's base for collision, the tentacle itself is just visual flair managed inside this entity
    [Collision(typeof(Player))]
    public class Tentacle : Entity
    {
        int t_index;

        Entity tentacle;

        IEnumerator state;

        public Tentacle(int index) : base(Vector2.Zero, new AnimatedSpriteRenderer("red_boss_warning", 10, 10, new Anim("move", new int[] { 0, 1 }, 8)), Drawing.DrawOrder.ENTITIES)
        {
            t_index = index;
            immovable = true;

            tentacle = new(Vector2.Zero, new AnimatedSpriteRenderer("red_boss_tentacle", 10, 64, new Anim("move", new int[] { 0, 1 }, 8)), Drawing.DrawOrder.ENTITIES)
            {
                exists = false
            };
        }

        public void Spawn(Touching dir, Red_Boss parent)
        {
            if (dir.HasFlag(Touching.UP))
            {
                Position = parent.Position + new Vector2(16 * t_index - 14, -13);
            }
            else if (dir.HasFlag(Touching.LEFT))
            {
                Position = parent.Position + new Vector2(-14, 16 * t_index - 16);
            }
            else if (dir.HasFlag(Touching.RIGHT))
            {
                Position = parent.Position + new Vector2(parent.width + 2, 16 * t_index - 16);
            }
            else if (dir.HasFlag(Touching.DOWN))
            {
                Position = parent.Position + new Vector2(16 * t_index - 14, parent.height + 2);
            }
            else
            {
                //player isn't close in any direction, so random locations
                Vector2 ul = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
                Position.X = ul.X + 16 + 12 * (1 + t_index) + GlobalState.RNG.Next(-5, 6);
                Position.Y = ul.Y + 16 * GlobalState.RNG.Next(1, 4) + GlobalState.RNG.Next(-5, 6) + tentacle.height - height;
            }

            tentacle.Position = Position + Vector2.UnitY * (height - 3 - tentacle.height);
            tentacle.y_push = tentacle.height;

            Flicker(0.7f + (float)GlobalState.RNG.NextDouble());
            state = StateLogic();
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(tentacle, 1);
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (!_flickering && other is Player p)
            {
                p.ReceiveDamage(1, Red_Boss.TentacleDamageDealer);
            }
        }

        public void Despawn()
        {
            exists = false;
            tentacle.exists = false;
        }

        IEnumerator StateLogic()
        {
            while (_flickering)
            {
                yield return null;
            }

            SoundManager.PlaySoundEffect("bubble_1", "bubble_1", "bubble_2", "bubble_3");
            tentacle.exists = true;

            float tentacle_drop_timer = 1f;

            while (!MathUtilities.MoveTo(ref tentacle.y_push, 0, 120))
            {
                tentacle_drop_timer -= GameTimes.DeltaTime;
                yield return null;
            }

            while (tentacle_drop_timer > 0)
            {
                tentacle_drop_timer -= GameTimes.DeltaTime;
                yield return null;
            }

            float end_timer = 0.3f;
            while (!MathUtilities.MoveTo(ref tentacle.y_push, tentacle.height, 180))
            {
                end_timer -= GameTimes.DeltaTime;
                yield return null;
            }

            while (end_timer > 0)
            {
                end_timer -= GameTimes.DeltaTime;
                yield return null;
            }

            exists = false;
            tentacle.exists = false;
            yield break;
        }
    }

    public class Ripple : Entity
    {
        Red_Boss parent;

        public Ripple(Red_Boss parent) : base(parent.Position, new AnimatedSpriteRenderer("red_boss_ripple", 48, 8, new Anim("a", new int[] { 0, 1 }, 12)), Drawing.DrawOrder.BG_ENTITIES)
        {
            this.parent = parent;
        }

        public override void Update()
        {
            base.Update();
            Position = parent.Position + new Vector2(-11, 17);
        }
    }

    [Collision(typeof(Player))]
    class SplashBullet : Entity
    {
        Parabola_Thing parabola;
        Vector2 startPos;

        IEnumerator state;

        public static AnimatedSpriteRenderer GetSprite() => new("red_boss_bullet", 8, 8,
            new Anim("move", new int[] { 0, 1 }, 12),
            new Anim("explode", new int[] { 2, 3, 4 }, 14, false)
            );

        public SplashBullet(Vector2 startPos) : base(startPos, GetSprite(), Drawing.DrawOrder.FG_SPRITES)
        {
            shadow = new Shadow(this, Vector2.Zero);
            this.startPos = startPos;
            parabola = new(this, 48, 1.2f + (float)GlobalState.RNG.NextDouble());
        }

        public void Spawn()
        {
            state = Logic();
            state.MoveNext();
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
        }

        IEnumerator Logic()
        {
            parabola.ResetTime();
            Position = startPos;
            velocity = Vector2.Zero;
            visible = false;
            shadow.visible = true;
            shadow.Flicker(0.5f);
            Play("move");

            while (shadow._flickering)
                yield return null;

            visible = true;
            SoundManager.PlaySoundEffect("bubble_1", "bubble_1", "bubble_2", "bubble_3");
            velocity.X = GlobalState.RNG.Next(10, 18);

            while (!parabola.Tick())
                yield return null;

            Play("explode");

            while (!AnimFinished)
                yield return null;

            SoundManager.PlaySoundEffect("bubble_1", "bubble_1", "bubble_2", "bubble_3");
            exists = false;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (visible && offset.Y < 10)
            {
                ((Player)other).ReceiveDamage(1, Red_Boss.BulletDamageDealer);
            }
        }
    }

    [Collision(typeof(Player))]
    public class ProximitySensor : Entity
    {
        Red_Boss parent;
        Touching direction;
        Vector2 offset_from_parent = Vector2.Zero;

        public ProximitySensor(Touching dir, Red_Boss parent) : base(Vector2.Zero)
        {
            visible = false;
            this.parent = parent;
            direction = dir;
            switch (dir)
            {
                case Touching.UP:
                    width = parent.width;
                    height = 8;
                    offset_from_parent = new(0, -21);
                    break;
                case Touching.DOWN:
                    width = parent.width + 4;
                    height = 12;
                    offset_from_parent = new(0, parent.height + 8);
                    break;
                case Touching.LEFT:
                    width = 12;
                    height = parent.height + 20;
                    offset_from_parent = new(-18, -10);
                    break;
                case Touching.RIGHT:
                    width = 8;
                    height = parent.height + 16;
                    offset_from_parent = new(parent.width + 7, -8);
                    break;
            }
        }

        public override void Update()
        {
            base.Update();
            Position = parent.Position + offset_from_parent;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            parent.proximity_hits |= direction;
        }
    }
}
