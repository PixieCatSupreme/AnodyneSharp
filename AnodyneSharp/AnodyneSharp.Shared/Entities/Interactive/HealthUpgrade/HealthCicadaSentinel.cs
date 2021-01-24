using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive
{
    [NamedEntity("Health_Cicada"), Collision(typeof(Player))]
    public class HealthCicadaSentinel : Entity
    {
        public bool PlayerCollided { get; private set; }
        public bool Activated { get; set; }

        public bool PlayerAway { get; set; }

        private HealthCicada _child;
        private Player _player;

        private float _flyDistance;

        public HealthCicadaSentinel(EntityPreset preset, Player player)
            : base(preset.Position, 16, 16, DrawOrder.ENTITIES)
        {
            Activated = false;
            visible = false;

            PlayerCollided = false;
            PlayerAway = false;

            _child = new HealthCicada(this, preset, player);

            _player = player;

            _flyDistance = 80;
        }

        public override void Update()
        {
            base.Update();

            if (!PlayerAway && Activated && (_player.Position - Position).Length() > _flyDistance)
            {
                PlayerAway = true;
                Activated = false;
            }

            _child.Update();
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            _child.PostUpdate();
        }

        public override void Collided(Entity other)
        {
            if (Activated && !PlayerCollided && _player.state == PlayerState.GROUND)
            {
                PlayerCollided = true;
                Activated = false;
            }
        }

        public override void Draw()
        {
            _child.RegisterDrawing();
        }
    }
}
