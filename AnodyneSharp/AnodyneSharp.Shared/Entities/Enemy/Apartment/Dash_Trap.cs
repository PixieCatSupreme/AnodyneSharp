using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Apartment
{
    [Collision(typeof(Player), typeof(Dust), KeepOnScreen = true, MapCollision = true)]
    public abstract class DashTrap : Entity
    {
        protected const float DashVel = 80;

        public const string DamageDealer = "Dash Trap";

        public static AnimatedSpriteRenderer GetSprite()
        {
            if (GlobalState.BoiEaster)
            {
                return new("dash_trap", 16, 16,
                    new Anim("idle", new int[] { 6 }, 1),
                    new Anim("dash", new int[] { 6 }, 1),
                    new Anim("bounce", new int[] { 6 }, 1));
            }
            else
            {
                return new("dash_trap", 16, 16,
                    new Anim("idle", new int[] { 4 }, 12),
                    new Anim("dash", new int[] { 5 }, 12),
                    new Anim("bounce", new int[] { 4,5 }, 12));
            }
        }

        public DashTrap(EntityPreset preset) : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            width = height = 14;
            CenterOffset();
            MapInteraction = false;
            HoleAsWall = true;
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                if (p.state != PlayerState.AIR)
                {
                    p.ReceiveDamage(1,DamageDealer);
                    OnPlayer();
                }
            }
            else
            {
                OnTouch();
            }
        }

        public override void Update()
        {
            base.Update();
            if (touching != Touching.NONE)
            {
                OnTouch();
            }
        }

        protected abstract void OnTouch();
        protected abstract void OnPlayer();
    }

    [NamedEntity("Dash_Trap", null, 1, 2)]
    class BounceDashTrap : DashTrap
    {
        public BounceDashTrap(EntityPreset preset, Player p) : base(preset)
        {
            Play("dash");
            if (preset.Frame == 1)
            {
                velocity.X = DashVel;
            }
            else
            {
                velocity.Y = DashVel;
            }
        }

        protected override void OnPlayer()
        {
            //Ignored
        }

        protected override void OnTouch()
        {
            Play("bounce");
            SoundManager.PlaySoundEffect("shieldy_ineffective");
            velocity = -velocity;
        }
    }

    [NamedEntity("Dash_Trap", null, 0), Collision(typeof(DashTrap))]
    class OnSightDashTrap : DashTrap
    {
        IState _state;
        Player _player;
        Vector2 _idlepos;

        public OnSightDashTrap(EntityPreset preset, Player p) : base(preset)
        {
            _player = p;
            _idlepos = Position;
            _state = new StateMachineBuilder()
                .State("Idle")
                    .Enter((s) => Play("idle"))
                    .Condition(SeePlayer, (s) => _state.ChangeState("Charging"))
                .End()
                .State("Charging")
                    .Enter((s) =>
                    {
                        FaceTowards(_player.Center);
                        velocity = FacingDirection(facing) * DashVel;
                        SoundManager.PlaySoundEffect("slasher_atk");
                        Play("dash");
                    })
                    .Event("bounce", (s) =>
                     {
                         SoundManager.PlaySoundEffect("shieldy_ineffective");
                         velocity = -velocity / 2;
                         _state.ChangeState("Returning");
                     })
                .End()
                .State("Returning")
                    .Condition(() => (_idlepos - Position).LengthSquared() < 4, (s) =>
                        {
                            velocity = Vector2.Zero;
                            Position = _idlepos;
                            _state.ChangeState("Idle");
                        })
                .End()
            .Build();
            _state.ChangeState("Idle");
        }

        public override void Update()
        {
            base.Update();
            _state.Update(GameTimes.DeltaTime);
        }

        bool SeePlayer()
        {
            bool seeX = _player.Hitbox.Left <= Hitbox.Right && _player.Hitbox.Right >= Hitbox.Left;
            bool seeY = _player.Hitbox.Top <= Hitbox.Bottom && _player.Hitbox.Bottom >= Hitbox.Top;
            return seeX || seeY;
        }

        protected override void OnPlayer()
        {
            _state.TriggerEvent("bounce");
        }

        protected override void OnTouch()
        {
            _state.TriggerEvent("bounce");
        }
    }
}
