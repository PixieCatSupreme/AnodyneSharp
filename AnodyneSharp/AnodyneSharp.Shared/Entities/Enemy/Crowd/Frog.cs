using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Crowd
{
    [NamedEntity, Enemy, Collision(typeof(Player),typeof(Broom))]
    class Frog : HealthDropper
    {
        EntityPreset _preset;
        int _health = 2;
        IState _state;

        Player _player;

        EntityPool<BurstBullet> _bullets;

        public Frog(EntityPreset preset, Player p) : base(preset.Position, "frog", 16, 16, Drawing.DrawOrder.ENTITIES, 0.7f)
        {
            AddAnimation("idle", CreateAnimFrameArray(0, 1), 2, true);
            AddAnimation("shoot_d", CreateAnimFrameArray(3), 3, false);
            AddAnimation("shoot_r", CreateAnimFrameArray(4), 3, false);
            AddAnimation("shoot_l", CreateAnimFrameArray(4), 3, false);
            AddAnimation("shoot_u", CreateAnimFrameArray(5), 3, false);
            Play("idle");
            immovable = true;
            _preset = preset;
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
                    .Condition(()=>(Center - _player.Center).LengthSquared() < 64 * 64, (s) =>
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
                        _preset.Alive = false;
                        GlobalState.SpawnEntity(new Explosion(this));
                        Die();
                    }
                }
            }
            else if(other is Player p && p.state != PlayerState.AIR)
            {
                p.ReceiveDamage(1);
            }
        }

        protected override void AnimationChanged(string name)
        {
            if(name == "shoot_l")
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
            return _bullets.Entities;
        }

        [Collision(typeof(Player))]
        private class BurstBullet : Entity
        {
            Parabola_Thing parabola;
            int _multiplier;

            IState _state;

            public BurstBullet(int speed_multiplier) : base(Vector2.Zero, "frog_bullet", 8, 8, Drawing.DrawOrder.FG_SPRITES)
            {
                _multiplier = speed_multiplier+1;
                
                parabola = new(this, 16, (float)(0.8 + GlobalState.RNG.NextDouble()));

                shadow = new Shadow(this, new Vector2(3, 2), ShadowType.Normal);

                AddAnimation("move", CreateAnimFrameArray(0, 1), 12);
                AddAnimation("explode", CreateAnimFrameArray(2, 3, 3), 10, false);

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
                        .Condition(() => _curAnim.Finished, (state) =>
                        {
                            exists = false;
                            SoundManager.PlaySoundEffect("bubble_1","bubble_1","bubble_2","bubble_3");
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
                MoveTowards(target.Center,15*_multiplier);
                _state.ChangeState("Move");
                parabola.ResetTime();
            }

            public override void Collided(Entity other)
            {
                if (other is Player p && offset.Y <= 8 && velocity != Vector2.Zero)
                {
                    p.ReceiveDamage(1);
                }
            }
        }
    }
}
