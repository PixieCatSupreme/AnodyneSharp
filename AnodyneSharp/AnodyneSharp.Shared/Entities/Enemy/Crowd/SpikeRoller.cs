using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Crowd
{
    [NamedEntity("Spike_Roller", type: null, 0, 1, 2, 3), Collision(typeof(Player))]
    public class FallingSpikeRoller : BaseSpikeRoller
    {
        RollerShadow _shadow;

        Vector2 _initPos;

        IState _state;

        public FallingSpikeRoller(EntityPreset preset, Player p)
            : base( preset, preset.Frame)
        {
            _shadow = new RollerShadow(Position - offset, preset.Frame);

            visible = false;

            offset.Y = 80;

            layer =  Drawing.DrawOrder.FG_SPRITES;

            _initPos = Position;

            _state = new StateMachineBuilder()
                .State("Hidden")
                    .Event("PlayerOverlaps", (state) =>
                    {
                        _state.ChangeState("Fall");
                    })
                .End()
                .State("Fall")
                    .Enter((state) =>
                    {
                        visible = true;
                        _shadow.visible = true;

                        _shadow.Flicker(-1);

                        SoundManager.PlaySoundEffect("fall_1");
                    })
                    .Condition(() => MathUtilities.MoveTo(ref offset.Y, 0, 60), (state) => _state.ChangeState("Roll"))
                    .Exit((state) =>
                    {
                        _shadow.exists = false;
                    })
                .End()
                .State("Roll")
                    .Enter((state) =>
                    {
                        CollisionOn = true;
                        velocity = vel;
                        layer = Drawing.DrawOrder.BG_ENTITIES;

                        ChangedVelocity();

                        Play("roll");

                        SoundManager.PlaySoundEffect("hit_ground_1");
                    })
                    .Condition(() => (_initPos - Position).Length() >= 112, (state) => _state.ChangeState("Stopped"))
                .End()
                .State("Stopped")
                    .Enter((state) =>
                    {
                        Play("idle");

                        velocity = Vector2.Zero;

                        SoundManager.PlaySoundEffect("hit_ground_1");
                    })
                .End()
            .Build();

            _state.ChangeState("Hidden");
        }

        public override void Update()
        {
            base.Update();

            _state.Update(GameTimes.DeltaTime);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() {_shadow };
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            _state.TriggerEvent("PlayerOverlaps");
        }

        public class RollerShadow : Entity
        {
            public RollerShadow(Vector2 pos, int frame)
                : base(pos, "spike_roller_shadow", 16, 128, Drawing.DrawOrder.ENTITIES)
            {

                switch (frame)
                {
                    case 0:
                        SetTexture("spike_roller_horizontal_shadow", 128, 16);

                        width = 128;
                        height = 16;
                        break;
                    case 3:
                        SetTexture("spike_roller_horizontal_shadow", 128, 16);
                        width = 128;
                        height = 16;

                        break;
                }

                visible = false;
            }
        }
    }

    [NamedEntity("Spike_Roller", type: null, 4, 5, 6, 7), Collision(typeof(Player), MapCollision = true)]
    public class BouncySpikeRoller : BaseSpikeRoller
    {
        public BouncySpikeRoller(EntityPreset preset, Player p)
            : base(preset, preset.Frame % 4)
        {
            CollisionOn = true;

            velocity = vel * 2;

            MapInteraction = false;
            immovable = false;

            Play("roll");

            ChangedVelocity();
        }

        public override void Update()
        {
            base.Update();

            if(touching != Touching.NONE)
            {
                velocity *= -1;

                SoundManager.PlaySoundEffect("hit_ground_1");

                ChangedVelocity();
            }
        }
    }

    public class BaseSpikeRoller : Entity
    {
        public const string DamageDealer = "Spike roller";
        protected Vector2 vel;

        public bool CollisionOn { get; protected set; }

        protected bool playerCollides;

        public static AnimatedSpriteRenderer GetSprite(int frame)
        {
            bool horiz = frame == 0 || frame == 3;
            return new(horiz ? "spike_roller_horizontal" : "spike_roller", horiz ? 128 : 16, horiz ? 16: 128,
                new Anim("idle", new int[] { 0 }, 1),
                new Anim("roll", new int[] { 0, 1 }, 5)
                );
        }
        public BaseSpikeRoller(EntityPreset preset, int frame)
            : base(preset.Position, GetSprite(frame), Drawing.DrawOrder.ROLLERS)
        {
            switch (frame)
            {
                case 0:
                    width = 128;
                    height = 12;
                    vel = new Vector2(0, -20);
                    break;
                case 1:
                    width = 12;
                    vel = new Vector2(20, 0);
                    break;
                case 2:
                    width = 12;
                    vel = new Vector2(-20, 0);
                    break;
                case 3:
                    width = 128;
                    height = 12;
                    vel = new Vector2(0, 20);
                    break;
            }

            CenterOffset();

            Position += offset;

            CollisionOn = false;
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (CollisionOn && other is Player p && p.state != PlayerState.AIR)
            {
                p.ReceiveDamage(1, DamageDealer);
            }
        }

        protected void ChangedVelocity()
        {
            if (velocity.X > 0)
            {
                _flip = SpriteEffects.None;
            }
            else if (velocity.X < 0)
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
            else if (velocity.Y < 0)
            {
                _flip = SpriteEffects.FlipVertically;
            }
            else
            {
                _flip = SpriteEffects.None;
            }
        }
    }
}
