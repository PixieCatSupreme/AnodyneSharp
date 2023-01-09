using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [Collision(typeof(Player))]
    public class KeyBlockSentinel : Entity, Interactable
    {
        private float _activationTime;
        public float _maxActivationTime;
        private bool _playerCollided;
        private bool _triedOpening;
        public bool OpensOnInteract = false;
        private KeyCardGate _gate;

        public KeyBlockSentinel(KeyCardGate gate) : 
            base(gate.Position - new Vector2(2,2), 20, 20)
        {
            visible = false;

            _activationTime = 0;
            _maxActivationTime = 0.2f;

            _gate = gate;

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
            if (OpensOnInteract || _triedOpening || (other is Player p && p.state != PlayerState.GROUND))
            {
                return;
            }

            _activationTime -= GameTimes.DeltaTime;
            _playerCollided = true;

            if (_activationTime < 0)
            {
                if(_gate.TryUnlock())
                {
                    _gate._preset.Alive = false;
                    exists = false;
                }
                _triedOpening = true;
            }
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if(!OpensOnInteract)
            {
                return false;
            }

            if(_gate.TryUnlock())
            {
                _gate._preset.Alive = false;
                exists = false;
            }
            return true;
        }
    }
}
