using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace AnodyneSharp.Entities.Enemy.Hotel
{
    [NamedEntity("Steam_Pipe"), Collision(typeof(Player), typeof(Dust), MapCollision = false)]
    class SteamPipe : Entity
    {
        private const float EjectVel = 50;

        private EntityPool<Steam> _steam;

        private Player _player;

        public bool Disabled;
        public bool PlayerCollided;

        public SteamPipe(EntityPreset preset, Player player)
            : base(preset.Position, "steam_pipe", 16, 16, DrawOrder.ENTITIES)
        {
            _player = player;

            Vector2 startPos = Vector2.Zero;

            SetFrame(preset.Frame);

            switch (Frame)
            {
                case 0:
                    width = 10;

                    offset = new Vector2(3, 16);
                    startPos = Position + new Vector2(2, 10);

                    facing = Facing.DOWN;
                    break;
                case 1:
                    height = 10;

                    offset = new Vector2(14, 5);
                    startPos = Position + new Vector2(14, 5);

                    facing = Facing.RIGHT;
                    break;
                case 2:
                    width = 10;

                    offset = new Vector2(3, -4);
                    startPos = Position + new Vector2(3, -2);

                    facing = Facing.UP;
                    break;
                case 3:
                    height = 10;

                    offset = new Vector2(-6, 3);
                    startPos = Position + new Vector2(-6, 3);

                    facing = Facing.LEFT;
                    break;
            }
            Position += offset;

            _steam = new EntityPool<Steam>(6, () => new Steam(startPos, this));

            _steam.Spawn(s => s.Spawn(), 6);
        }

        public override void Update()
        {
            base.Update();

            if (PlayerCollided && !Disabled)
            {
                PushPlayer(_player);
            }

            if (Disabled)
            {
                foreach (Steam s in _steam.Entities)
                {
                    s.Disabled = true;
                }
            }

            Disabled = false;
            PlayerCollided = false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return _steam.Entities;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (Disabled)
            {
                return;
            }

            if (other is Player)
            {
                PlayerCollided = true;
            }
            else if (other is Dust d && !d.IS_RAFT)
            {
                Disabled = true;
            }
        }

        private void PushPlayer(Player p)
        {
            switch (facing)
            {
                case Facing.DOWN:
                    p.additionalVel.Y = EjectVel;
                    break;
                case Facing.UP:
                    p.additionalVel.Y = -EjectVel;
                    break;
                case Facing.LEFT:
                    p.additionalVel.X = -EjectVel;
                    break;
                default:
                    p.additionalVel.X = EjectVel;
                    break;
            }
        }

        [Collision(typeof(Player), MapCollision = false)]
        private class Steam : Entity
        {
            private float _timer;

            public bool Disabled;

            private Vector2 _startPos;

            private SteamPipe _parent;

            public Steam(Vector2 startPos, SteamPipe parent)
                : base(Vector2.Zero, "steam", 16, 16, DrawOrder.FG_SPRITES)
            {
                _startPos = startPos;

                AddAnimation("s", CreateAnimFrameArray(0, 1), 10);


                scale = GlobalState.RNG.Next(8, 10) / 10f;
                width = height = (int)(width * scale);

                _parent = parent;

                facing = parent.facing;
            }

            public void Spawn()
            {
                if (!Disabled)
                {
                    _timer = GlobalState.RNG.Next(30, 51) / 60f;
                }
                else
                {
                    _timer = GlobalState.RNG.Next(3, 6) / 60f;
                }

                Position = _startPos;

                switch (facing)
                {
                    case Facing.DOWN:
                        velocity.Y = EjectVel + GlobalState.RNG.Next(0, 21);
                        velocity.X = GlobalState.RNG.Next(-30, 31);
                        break;
                    case Facing.UP:
                        velocity.Y = -EjectVel + GlobalState.RNG.Next(-20, 1);
                        velocity.X = GlobalState.RNG.Next(-30, 31);
                        break;
                    case Facing.LEFT:
                        velocity.X = -EjectVel + GlobalState.RNG.Next(-20, 1);
                        velocity.Y = GlobalState.RNG.Next(-30, 31);
                        break;
                    case Facing.RIGHT:
                        velocity.X = EjectVel + GlobalState.RNG.Next(0, 21);
                        velocity.Y = GlobalState.RNG.Next(-30, 31);
                        break;
                }

                _startPos = Position;

                Play("s");

                SoundManager.PlaySoundEffect("fireball");
            }

            public override void Update()
            {
                base.Update();

                _timer -= GameTimes.DeltaTime;

                if (_timer <= 0)
                {
                    Spawn();
                }

                Disabled = false;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);

                if (other is Player)
                {
                    _parent.PlayerCollided = true;
                }
            }
        }
    }
}
