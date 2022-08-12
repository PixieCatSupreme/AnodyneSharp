using AnodyneSharp.GameEvents;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Cell
{
    [NamedEntity("Chaser"), Collision(typeof(Player), KeepOnScreen = true, MapCollision = true), Events(typeof(BroomUsed))]
    class Chaser : Entity
    {
        Player _player;

        bool _isHorizontal;

        float _targetVel;

        public Chaser(EntityPreset preset, Player p)
            : base(preset.Position, "chaser", 16, 32, Drawing.DrawOrder.ENTITIES)
        {
            _player = p;

            width = height = 8;

            offset = new Vector2(4, 20);

            Position += new Vector2(4,20);

            AddAnimation("walking_d", CreateAnimFrameArray(4, 5), 8, true);
            AddAnimation("walking_r", CreateAnimFrameArray(6, 7), 8, true);
            AddAnimation("walking_u", CreateAnimFrameArray(8, 9), 8, true);
            AddAnimation("walking_l", CreateAnimFrameArray(10, 11), 8, true);

            _isHorizontal = preset.Frame == 0;

            if (_isHorizontal)
            {
                SetFrame(6);
            }
            else
            {
                SetFrame(4);
            }
        }

        public override void Update()
        {
            base.Update();

            if (velocity == Vector2.Zero)
            {
                CheckStartMove();
            }
            else
            {
                if (touching != Touching.NONE)
                {
                    velocity *= -1;

                    facing = FlipFacing(facing);

                    PlayFacing("walking");
                }

                if (_targetVel != 0)
                {
                    SpeedUp();
                }
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Player p)
            {
                p.ReceiveDamage(6);
            }
        }

        public override void OnEvent(GameEvent e)
        {
            base.OnEvent(e);

            SoundManager.PlaySoundEffect("mover_move");

            _targetVel = velocity.Length() * 1.6f;

            if (_targetVel > 100)
            {
                _targetVel = 100;
            }
        }

        private void CheckStartMove()
        {
            if (_isHorizontal)
            {
                if (_player.Position.Y > Position.Y - _player.height && _player.Position.Y < Position.Y + height)
                {
                    FaceTowards(_player.Position);

                    velocity = new Vector2(15, 0) * FacingDirection(facing);

                    if (velocity.X != 0)
                    {
                        PlayFacing("walking");
                    }
                }
            }
            else
            {
                if (_player.Position.X > Position.X - _player.width && _player.Position.X < Position.X + width)
                {
                    FaceTowards(_player.Position);

                    velocity = new Vector2(0, 15) * FacingDirection(facing);

                    if (velocity.Y != 0)
                    {
                        PlayFacing("walking");
                    }
                }
            }
        }

        private void SpeedUp()
        {
            if (_isHorizontal)
            {
                if (MathUtilities.MoveTo(ref velocity.X, _targetVel * MathF.Sign(velocity.X), _targetVel * (0.6f / 1.6f) / 0.3f))
                {
                    _targetVel = 0;
                }
            }
            else
            {
                if (MathUtilities.MoveTo(ref velocity.Y, _targetVel * MathF.Sign(velocity.Y), _targetVel * (0.6f / 1.6f) / 0.3f))
                {
                    _targetVel = 0;
                }
            }
        }
    }
}
