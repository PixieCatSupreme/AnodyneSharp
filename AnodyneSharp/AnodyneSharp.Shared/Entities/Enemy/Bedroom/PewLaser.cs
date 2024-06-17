using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.FSM;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Bedroom
{
    [NamedEntity("Pew_Laser")]
    public class PewLaser : Entity
    {
        private const float _bulletTimerMax = 0.5f;
        public const string DamageDealer = "Pew Laser";


        private EntityPool<Laser> lasers;

        private float _bulletTimer = _bulletTimerMax*2f;

        public PewLaser(EntityPreset preset, Player player)
            : base(preset.Position, "pew_laser", 16, 16, DrawOrder.ENTITIES)
        {
            immovable = true;


            bool isFast = false;
            int frame = preset.Frame % 4;

            SetFrame(frame);

            if (preset.Frame >= 4)
            {
                isFast = true;
            }

            facing = (frame) switch
            {
                1 => Facing.RIGHT,
                2 => Facing.UP,
                3 => Facing.LEFT,
                _ => Facing.DOWN,
            };

            lasers = new EntityPool<Laser>(5, () => new Laser(facing, isFast));
        }

        public override void Update()
        {
            base.Update();

            _bulletTimer -= GameTimes.DeltaTime;

            if (_bulletTimer < 0)
            {
                _bulletTimer = _bulletTimerMax;
                lasers.Spawn((f) => f.Spawn(this));
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return lasers.Entities;
        }

        [Collision(typeof(Player), typeof(Dust), typeof(Shieldy), MapCollision = true)]
        class Laser : Entity
        {
            public static AnimatedSpriteRenderer GetSprite() => new("pew_laser_bullet", 16, 8,
                    new Anim("shoot", new int[] { 0, 1 }, 8),
                    new Anim("poof", new int[] { 4, 5, 6, 7 }, 8, looped: false));

            public const float BULLET_VELOCITY = 40;
		    public const float BULLET_FAST_VELOCITY = 70;
            private IState _state;
            private bool _isFast;
            //timer to re-enable being solid to avoid being inside the wall
            private float solid_timer;

            private Vector2 spawn_offset;

            public Laser(Facing facing, bool isFast) 
                : base(Vector2.Zero, GetSprite(), DrawOrder.FG_SPRITES)
            {   
                MapInteraction = false;

                this.facing = facing;
                _isFast = isFast;

                switch (facing)
                {
                    case Facing.DOWN:
                        spawn_offset = new Vector2(0, 11);
                        break;
                    case Facing.UP:
                        spawn_offset = new Vector2(0, 8);
                        rotation = MathF.PI;
                        break;
                    case Facing.RIGHT:
                        (width, height) = (height, width);

                        spawn_offset = Vector2.Zero;
                        offset = new Vector2(4, -4);
                        rotation = -MathF.PI / 2;
                        break;
                    case Facing.LEFT:
                        (width, height) = (height, width);

                        spawn_offset = new Vector2(8, 0);
                        offset = new Vector2(4, -4);
                        rotation = MathF.PI / 2;
                        break;
                }

                _state = new StateMachineBuilder()
                    .State("Shoot")
                        .Enter((state) => Play("shoot"))
                        .Update((state, time) => opacity -= 0.06f * time)
                        .Condition(() => touching != Touching.NONE, (s) => _state.ChangeState("Poof"))
                        .Event<CollisionEvent<Player>>("Player", (s, p) => { p.entity.ReceiveDamage(1, DamageDealer); _state.ChangeState("Poof"); })
                        .Event<CollisionEvent<Shieldy>>("Shieldy", (s, p) => { p.entity.ReceiveDamage(5); _state.ChangeState("Poof"); })
                        .Event("Dust", (s) => { _state.ChangeState("Poof"); })
                    .End()
                    .State("Poof")
                        .Enter((state) => { Play("poof"); velocity = Vector2.Zero; })
                        .Condition(() => AnimFinished, (s) => exists = false)
                    .End()
                    .Build();
            }

            public void Spawn(Entity parent)
            {
                Position = parent.Position + spawn_offset;
                opacity = 1.0f;

                Solid = false;
                solid_timer = _isFast ? 0.2f : 0.4f;

                SoundManager.PlaySoundEffect("laser-pew");

                _state.ChangeState("Shoot");

                velocity = FacingDirection(facing) * (_isFast ? BULLET_FAST_VELOCITY : BULLET_VELOCITY);
            }

            public override void Collided(Entity other)
            {
                if (other is Player p)
                {
                    _state.TriggerEvent("Player", new CollisionEvent<Player>() { entity = p });
                }
                else if (other is Shieldy s)
                {
                    _state.TriggerEvent("Shieldy", new CollisionEvent<Shieldy>() { entity = s });
                }
                else if (other is Dust d)
                {
                    _state.TriggerEvent("Dust");
                }
            }

            public override void Update()
            {
                _state.Update(GameTimes.DeltaTime);
                
                solid_timer -= GameTimes.DeltaTime;
                if (solid_timer < 0) Solid = true;

                base.Update();
            }
        }
    }
}
