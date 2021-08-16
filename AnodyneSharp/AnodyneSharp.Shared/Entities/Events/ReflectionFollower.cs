using AnodyneSharp.Drawing;
using AnodyneSharp.GameEvents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Events
{
    [NamedEntity("Event", null, 7), Events(typeof(StartWarp)), Collision(MapCollision = true)]
    public class ReflectionFollower : Entity
    {
        Player _player;
        Entity _broomReflection;


        public ReflectionFollower(EntityPreset preset, Player p)
            : base(preset.Position, "young_player_reflection", 16, 16, DrawOrder.PLAYER_REFLECTION)
        {
            _player = p;

            _broomReflection = new Entity(preset.Position, "broom_reflection", 16, 16, DrawOrder.BROOM_REFLECTION);
            _broomReflection.exists = false;

            if (p.follower != null)
            {
                exists = false;
                return;
            }

            p.follower = this;
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            Position = _player.Position + new Vector2(0, 7);

            offset = new Vector2(_player.offset.X, -_player.offset.Y);

            SetFrame(_player.GetFrame());

            _broomReflection.visible = visible;

            _broomReflection.exists = _player.broom.exists;

            if (_broomReflection.exists)
            {
                _broomReflection.Position = _player.broom.Position + new Vector2(0, 10);
                _broomReflection.offset = _player.broom.offset;

                _broomReflection.SetFrame(_player.broom.GetFrame());

                _broomReflection.layer = _player.broom.is_behind_player ? DrawOrder.BROOM_REFLECTION_BEHIND : DrawOrder.BROOM_REFLECTION;

                SetBroomRotation(_player.broom.facing);
            }
        }

        private void SetBroomRotation(Facing facing)
        {
            float r;

            switch (facing)
            {

                case Facing.RIGHT:
                    r = MathHelper.ToRadians(180);
                    break;
                case Facing.UP:
                    r = MathHelper.ToRadians(90);
                    break;
                case Facing.DOWN:
                    r = MathHelper.ToRadians(270);
                    break;
                default:
                    r = 0;
                    break;
            }

            _broomReflection.rotation = r;

        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(_broomReflection, 1);
        }

        public override void OnEvent(GameEvent e)
        {
            base.OnEvent(e);

            exists = false;
            _player.follower = null;
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
