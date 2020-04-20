using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity, Collision(typeof(Player))]
    public class KeyBlock : Entity
    {
        EntityPreset _preset;

        private float _activationTime;
        private float _maxActivationTime;
        private bool _playerCollided;
        private GameObject collider;

        //It's just the small key block for now!
        public KeyBlock(EntityPreset preset) : 
            base(preset.Position, 16, 16, Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;

            collider = new GameObject(preset.Position, 16,16)
            {
                solid = true,
                immovable = true
            };

            SetTexture("keyhole");
            SetFrame(_preset.Frame);

            AddAnimation("open", CreateAnimFrameArray(16, 17,18,19,20), 10, false);

            _activationTime = 0;
            _maxActivationTime = 0.2f;

            width = 20;
            height = 20;

            offset = new Vector2(-2);
            Position += offset;

            _playerCollided = false;

            collider.Update();
            collider.PostUpdate();
        }

        public override void Update()
        {
            if (!_playerCollided)
            {
                _activationTime = _maxActivationTime;
            }

            _playerCollided = false;

            base.Update();
        }

        public override void Collided(Entity other)
        {
            if (!_preset.Alive)
            {
                return;
            }

            Separate(collider, other);

            _activationTime -= GameTimes.DeltaTime;
            _playerCollided = true;

            if (_activationTime < 0)
            {
                if (InventoryState.GetCurrentMapKeys() > 0)
                {
                    Play("open");
                    InventoryState.RemoveCurrentMapKey();
                    _preset.Alive = false;
                }
                else
                {
                    //TODO play keyblock dialogue
                }
            }
        }
    }
}
