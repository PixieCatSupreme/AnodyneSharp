using AnodyneSharp.Entities.Events;
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
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom), MapCollision = true)]
    public class Red_Boss : Entity
    {
        IEnumerator state;
        Player player;
        EntityPreset preset;
        Ripple ripple;

        EntityPool<SplashBullet> splash_bullets;

        ProximitySensor[] sensors;
        public Touching proximity_hits = Touching.NONE;

        int health = 12;
        float invincible_timer = 0f;

        const float push_tick_max = 0.15f;

        float amp = 0;
        float pushdown_timer = 0f;

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

        public Red_Boss(EntityPreset preset, Player p) : base(preset.Position, "red_boss", 32, 32, Drawing.DrawOrder.ENTITIES)
        {
            height = 19;
            width = 26;
            offset = new Vector2(3, 13);

            AddAnimation("bob", CreateAnimFrameArray(0), 20);
            AddAnimation("close_eyes", CreateAnimFrameArray(1), 10, false);
            AddAnimation("warn", CreateAnimFrameArray(2), 24);
            AddAnimation("die", CreateAnimFrameArray(0, 1, 2, 1), 3, false);

            Play("close_eyes");

            ripple = new(this);
            Vector2 tl = MapUtilities.GetRoomUpperLeftPos(MapUtilities.GetRoomCoordinate(Position));
            Point splash_start = new(2, 2);
            splash_bullets = new(4, () => {
                splash_start.Y++;
                return new(tl + splash_start.ToVector2() * 16 + Vector2.One*3);
            });

            sensors = new ProximitySensor[]
            {
                new(Touching.LEFT,this),
                new(Touching.RIGHT,this),
                new(Touching.UP,this),
                new(Touching.DOWN,this),
            };

            player = p;
            this.preset = preset;

            state = State();

        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { ripple }.Concat(splash_bullets.Entities).Concat(sensors);
        }

        public override void Update()
        {
            base.Update();
            invincible_timer -= GameTimes.DeltaTime;
            state.MoveNext();
            proximity_hits = Touching.NONE;
        }

        public override void Collided(Entity other)
        {
            if (health == 0) return;

            if (other is Player p)
            {
                p.ReceiveDamage(1);
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
            y_push = sprite.Height;
            player.grid_entrance = MapUtilities.GetRoomUpperLeftPos(MapUtilities.GetRoomCoordinate(Position)) + Vector2.One * 20;

            GlobalState.SpawnEntity(new VolumeEvent(0, 3));

            while (MapUtilities.GetInGridPosition(player.Position).X < 48)
            {
                yield return null;
            }

            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("redboss", "before_fight");

            float push_timer = 0f;
            while (!GlobalState.LastDialogueFinished)
            {
                SoundManager.PlaySoundEffect("bubble_loop"); //Call each frame to get looping behavior out of a sound effect
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
                        //TODO: spawn tentacles
                        if(proximity_hits != Touching.NONE)
                        {
                            s.got_too_close++;
                            if(s.got_too_close == 2)
                            {
                                s.got_too_close = 0;
                                s.Parent.ChangeState("Dash"); //TODO: should be stun
                            }
                        }
                    })
                .End()
                .State<DashState>("Dash")
                    .Enter((s) => {
                        amp = 0;
                        velocity = new Vector2(30, 20);
                    })
                    .Condition(()=>touching != Touching.NONE,(s) =>
                    {
                        Drawing.Effects.ScreenShake.Directions dirs = new();
                        if(touching.HasFlag(Touching.UP))
                        {
                            velocity.Y = 60;
                            dirs |= Drawing.Effects.ScreenShake.Directions.Vertical;
                        }
                        else if(touching.HasFlag(Touching.DOWN))
                        {
                            velocity.Y = -60;
                            dirs |= Drawing.Effects.ScreenShake.Directions.Vertical;
                        }

                        if (touching.HasFlag(Touching.LEFT))
                        {
                            velocity.X = 60;
                            dirs |= Drawing.Effects.ScreenShake.Directions.Horizontal;
                        }
                        else if(touching.HasFlag(Touching.RIGHT))
                        {
                            velocity.X = -60;
                            dirs |= Drawing.Effects.ScreenShake.Directions.Horizontal;
                        }
                        GlobalState.screenShake.Shake(0.05f, 0.1f, dirs);
                    })
                    .Event("Tentacles",(s) =>
                    {
                        //TODO: spawn tentacles
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
                    .Exit((s) => velocity = Vector2.Zero)
                .End()
                .State("Stun")
                //TODO: stun wave state
                .End()
                .Build();

            state.ChangeState("Splash");
            
            while (health > 0)
            {
                state.Update(GameTimes.DeltaTime);

                pushdown_timer += GameTimes.DeltaTime * 3;
                y_push = amp + MathF.Sin(pushdown_timer) * amp;
                yield return null;
            }

            //Kill all subentities

            SoundManager.StopSong();
            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("redboss", "after_fight");
            GlobalState.screenShake.Shake(0.05f, 0.1f);
            GlobalState.flash.Flash(1f, Color.Red);

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
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
            SoundManager.PlaySong("redcave");
            GlobalState.events.BossDefeated.Add("REDCAVE");
            yield break;
        }
    }

    class Ripple : Entity
    {
        Red_Boss parent;

        public Ripple(Red_Boss parent) : base(parent.Position, "red_boss_ripple", 48, 8, Drawing.DrawOrder.BG_ENTITIES)
        {
            AddAnimation("a", CreateAnimFrameArray(0, 1), 12);
            Play("a");
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

        public SplashBullet(Vector2 startPos) : base(startPos, "red_boss_bullet", 8, 8, Drawing.DrawOrder.FG_SPRITES)
        {
            shadow = new Shadow(this, Vector2.Zero);
            AddAnimation("move", CreateAnimFrameArray(0, 1), 12);
            AddAnimation("explode", CreateAnimFrameArray(2, 3, 4), 14, false);
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

            while (!_curAnim.Finished)
                yield return null;

            SoundManager.PlaySoundEffect("bubble_1", "bubble_1", "bubble_2", "bubble_3");
            exists = false;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if(visible && offset.Y < 10)
            {
                ((Player)other).ReceiveDamage(1);
            }
        }
    }

    [Collision(typeof(Player))]
    class ProximitySensor : Entity
    {
        Red_Boss parent;
        Touching direction;
        Vector2 offset_from_parent = Vector2.Zero;

        public ProximitySensor(Touching dir, Red_Boss parent) : base(Vector2.Zero,Drawing.DrawOrder.ENTITIES)
        {
            visible = false;
            this.parent = parent;
            direction = dir;
            switch(dir)
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
