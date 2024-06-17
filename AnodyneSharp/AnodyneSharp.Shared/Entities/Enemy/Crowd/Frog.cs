using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Crowd
{
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom))]
    class Frog : HealthDropper
    {
        public static string DamageDealer = "Frog";
        public static string BulletDamageDealer = "Frog bullet";
        public static AnimatedSpriteRenderer GetSprite()
        {
            Anim[] anims;
            if (GlobalState.BoiEaster)
            {
                anims = new Anim[]
                {
                    new("idle", new int[]{ 6, 7 }, 2),
                    new("shoot_d", new int[]{8}, 3, false),
                    new("shoot_r", new int[]{8}, 3, false),
                    new("shoot_l", new int[]{8}, 3, false),
                    new("shoot_u", new int[]{8}, 3, false)
            };
            }
            else
            {
                anims = new Anim[]
                {
                    new("idle", new int[]{ 0,1 }, 2),
                    new("shoot_d", new int[]{3}, 3, false),
                    new("shoot_r", new int[]{4}, 3, false),
                    new("shoot_l", new int[]{4}, 3, false),
                    new("shoot_u", new int[]{5}, 3, false)
                };
            }
            return new("frog", 16, 16, anims);
        }

        int _health = 2;
        IState _state;

        Player _player;

        EntityPool<BurstBullet> _bullets;

        public Frog(EntityPreset preset, Player p) : base(preset, preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES, 0.7f)
        {
            immovable = true;
            _player = p;

            int i = 0;
            _bullets = new(3, () => new(i++));

            _state = new StateMachineBuilder()
                .State<TimerState>("Initial")
                    .Enter((s) =>
                    {
                        s.Reset();
                        s.AddTimer(0.8f, "goToIdle");
                    })
                    .Event("goToIdle", (s) => _state.ChangeState("Idle"))
                .End()
                .State("Idle")
                    .Enter((s) =>
                    {
                        Play("idle");
                    })
                    .Condition(() => (Center - _player.Center).LengthSquared() < 64 * 64, (s) =>
                    {
                        _bullets.Spawn((b) => b.Spawn(this, _player), 3);
                        FaceTowards(_player.Center);
                        PlayFacing("shoot");
                        SoundManager.PlaySoundEffect("bubble_triple");
                        _state.ChangeState("BulletsFlying");
                    })
                .End()
                .State("BulletsFlying")
                    .Condition(() => _bullets.Alive == 0, (s) => _state.ChangeState("Idle"))
                .End()
            .Build();
            _state.ChangeState("Initial");
        }

        public override void Update()
        {
            base.Update();
            _state.Update(GameTimes.DeltaTime);
        }

        public override void Collided(Entity other)
        {
            if (other is Broom)
            {
                if (!_flickering)
                {
                    --_health;
                    Flicker(1);

                    SoundManager.PlaySoundEffect("broom_hit");

                    if (_health <= 0)
                    {
                        GlobalState.SpawnEntity(new Explosion(this));
                        Die();
                    }
                }
            }
            else if (other is Player p && p.state != PlayerState.AIR)
            {
                p.ReceiveDamage(1, DamageDealer);
            }
        }

        protected override void AnimationChanged(string name)
        {
            if (name == "shoot_l")
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
            return base.SubEntities().Concat(_bullets.Entities);
        }

        [Collision(typeof(Player))]
        private class BurstBullet : Entity
        {
            Parabola_Thing parabola;
            int _multiplier;

            IState _state;

            public static AnimatedSpriteRenderer GetSprite()
            {
                if (GlobalState.BoiEaster) return new("frog_bullet", 8, 8, new Anim("move", new int[] { 4, 5 }, 12), new Anim("explode", new int[] { 2, 3, 3 }, 10, false));
                return new("frog_bullet", 8, 8, new Anim("move", new int[] { 0, 1 }, 12), new Anim("explode", new int[] { 2, 3, 3 },10, false));
            }

            public BurstBullet(int speed_multiplier) : base(Vector2.Zero, GetSprite(), Drawing.DrawOrder.FG_SPRITES)
            {
                _multiplier = speed_multiplier + 1;

                parabola = new(this, 16, (float)(0.8 + GlobalState.RNG.NextDouble()));

                shadow = new Shadow(this, new Vector2(3, 2), ShadowType.Normal);

                _state = new StateMachineBuilder()
                    .State("Move")
                        .Enter((state) =>
                        {
                            Play("move");
                            shadow.exists = true;
                            opacity = 1.0f;
                        })
                        .Update((state, time) =>
                        {
                            if (parabola.Tick())
                            {
                                _state.ChangeState("Splash");
                            }
                        })
                    .End()
                    .State("Splash")
                        .Enter((state) =>
                        {
                            shadow.exists = false;
                            Play("explode");
                            velocity = Vector2.Zero;
                        })
                        .Condition(() => AnimFinished, (state) =>
                        {
                            exists = false;
                            SoundManager.PlaySoundEffect("bubble_1", "bubble_1", "bubble_2", "bubble_3");
                        })
                    .End()
                    .Build();
            }

            public override void Update()
            {
                base.Update();
                _state.Update(GameTimes.DeltaTime);
            }

            public void Spawn(Frog parent, Player target)
            {
                Position = parent.Position;
                MoveTowards(target.Center, 15 * _multiplier);
                _state.ChangeState("Move");
                parabola.ResetTime();
            }

            public override void Collided(Entity other)
            {
                if (other is Player p && offset.Y <= 8 && velocity != Vector2.Zero)
                {
                    p.ReceiveDamage(1, BulletDamageDealer);
                }
            }
        }
    }
}
