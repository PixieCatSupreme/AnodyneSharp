using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Gadget.Gates;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _triedOpening;
        private KeyCardGate _gate;

        //It's just the small key block for now!
        public KeyBlock(EntityPreset preset, Player p) : 
            base(preset.Position - new Vector2(2,2), 20, 20, DrawOrder.ENTITIES)
        {
            visible = false;
            _preset = preset;

            _activationTime = 0;
            _maxActivationTime = 0.2f;

            _gate = new SmallKeyGate(preset.Position);

            _playerCollided = false;
            _triedOpening = false;
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
            if (!_preset.Alive || _triedOpening || (other is Player p && p.state != PlayerState.GROUND))
            {
                return;
            }

            _activationTime -= GameTimes.DeltaTime;
            _playerCollided = true;

            if (_activationTime < 0)
            {
                if(_gate.TryUnlock())
                {
                    _gate.Play("Open");
                    _preset.Alive = false;
                }
                _triedOpening = true;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(_gate,1);
        }
    }
}
