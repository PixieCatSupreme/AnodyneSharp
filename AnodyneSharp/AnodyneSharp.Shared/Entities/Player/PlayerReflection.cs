using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.GameEvents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities
{
    [Collision(MapCollision = true)]
    public class PlayerReflection : Entity
    {
        Player _player;
        Entity _broomReflection;


        public PlayerReflection(Player p)
            : base(p.Position, "young_player_reflection", 16, 16, DrawOrder.PLAYER_REFLECTION)
        {
            _player = p;

            _broomReflection = new Entity(p.Position, "broom_reflection", 16, 16, new RefLayer(layer_def, 1))
            {
                exists = false
            };
        }

        public override void Update()
        {
            HasVisibleHitbox = false;
            base.Update();

            Position = _player.Position + new Vector2(0, 7);

            offset = new Vector2(_player.offset.X, -_player.offset.Y);

            SetFrame(_player.Frame);

            _broomReflection.visible = visible;

            _broomReflection.exists = _player.broom.exists;

            if (_broomReflection.exists)
            {
                _broomReflection.Position = _player.broom.Position + new Vector2(0, 10);
                _broomReflection.offset = _player.broom.offset;

                _broomReflection.SetFrame(_player.broom.Frame);

                _broomReflection.layer_def = new RefLayer(layer_def, _player.broom.is_behind_player ? -1 : 1);

                SetBroomRotation(_player.broom.facing);
            }
        }

        private void SetBroomRotation(Facing facing)
        {
            var r = facing switch
            {
                Facing.RIGHT => MathHelper.ToRadians(180),
                Facing.UP => MathHelper.ToRadians(90),
                Facing.DOWN => MathHelper.ToRadians(270),
                _ => 0,
            };
            _broomReflection.rotation = r;

        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(_broomReflection, 1);
        }

        public override void Fall(Vector2 fallPoint)
        { }

        public override void Reflection()
        {
            base.Reflection();

            visible = true;
        }

        public override void Draw()
        {
            base.Draw();

            visible = false;
        }
    }
}
