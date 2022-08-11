using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Entities.Enemy.Circus
{
    [NamedEntity("Circus_Folks", null, 2), Collision(typeof(Dust))]
    internal class CircusFolks : Entity
    {
        private const int walkVel = 90;

        private EntityPreset _preset;
        private Player _player;
        private Parabola_Thing _parabolaThing;

        private Arthur _arthur;
        private Javiera _javiera;
        private EntityPool<ShockWave> _shockWaves;

        private IEnumerator _stateLogic;

        private Vector2 _topLeft;

        private int health = 7;

        public CircusFolks(EntityPreset preset, Player p)
            : base(preset.Position, "arthur_javiera", 16, 32, Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;

            _topLeft = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(16);

            _player.grid_entrance = _topLeft + new Vector2(75, 16 * 7 + 4);

            height = 16;
            offset.Y = 16;

            _arthur = new Arthur(_topLeft + new Vector2(64 - 8 - 16, 0));
            _javiera = new Javiera(_topLeft + new Vector2(64 + 8, 0));

            AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 8);
            AddAnimation("walk_u", CreateAnimFrameArray(2, 3), 12);
            AddAnimation("walk_r", CreateAnimFrameArray(4, 5), 12);
            AddAnimation("walk_l", CreateAnimFrameArray(4, 5), 12);
            AddAnimation("switch", CreateAnimFrameArray(6, 7), 6, false);
            AddAnimation("throw_d", CreateAnimFrameArray(8, 8), 1, false); // The length of all frames minus last is how long the warning lasts.
            AddAnimation("throw_r", CreateAnimFrameArray(10, 8), 1, false);
            AddAnimation("throw_u", CreateAnimFrameArray(9, 8), 1, false);
            AddAnimation("throw_l", CreateAnimFrameArray(10, 8), 1, false);
            AddAnimation("dying", CreateAnimFrameArray(10, 10, 10), 1, false);
            Play("walk_l");

            visible = false;

            _parabolaThing = new Parabola_Thing(this, 48, 1);
            shadow = new Shadow(this, new Vector2(0, 0), ShadowType.Normal);

            _shockWaves = new EntityPool<ShockWave>(8, () => new ShockWave());

            _stateLogic = Intro();
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { _arthur, _javiera }.Concat(_shockWaves.Entities);
        }

        public override void Update()
        {
            base.Update();

            _stateLogic.MoveNext();
        }

        protected override void AnimationChanged(string name)
        {
            base.AnimationChanged(name);
            if (name.EndsWith('l'))
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = SpriteEffects.None;
            }
        }

        private IEnumerator Intro()
        {
            while (_player.Position.Y > _player.grid_entrance.Y)
            {
                yield return null;
            }

            VolumeEvent volumeEvent = new VolumeEvent(0, 0.6f);

            GlobalState.SpawnEntity(volumeEvent);

            GlobalState.Dialogue = DialogueManager.GetDialogue("circus_folks", "before_fight");

            while (!volumeEvent.ReachedTarget || !GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            SoundManager.PlaySong("circus-boss");

            _arthur.Play("walk_r");
            _arthur.velocity.X = 20;

            _javiera.Play("walk_l");
            _javiera.velocity.X = -20;

            while (_arthur.Position.X < _topLeft.X + 64)
            {
                yield return null;
            }

            _arthur.velocity = _javiera.velocity = Vector2.Zero;
            _arthur.exists = _javiera.exists = false;

            visible = true;
            Play("walk_l");

            Position = _topLeft + new Vector2(56,0);

            //velocity.X = -walkVel;

            yield break;
        }

        class Arthur : Entity
        {
            private Parabola_Thing _parabolaThing;
            public Arthur(Vector2 position)
                : base(position, "arthur", 16, 16, Drawing.DrawOrder.ENTITIES)
            {
                AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 8);
                AddAnimation("walk_l", CreateAnimFrameArray(4, 5), 8);
                AddAnimation("walk_u", CreateAnimFrameArray(2, 3), 8);
                AddAnimation("walk_r", CreateAnimFrameArray(4, 5), 8);
                AddAnimation("roll", CreateAnimFrameArray(6), 6); // For flying through the air
                AddAnimation("stunned", CreateAnimFrameArray(8, 9), 6);
                AddAnimation("wobble", CreateAnimFrameArray(16, 17), 8);
                AddAnimation("fall_1", CreateAnimFrameArray(10), 8);
                AddAnimation("fall", CreateAnimFrameArray(10, 11, 12, 13, 14, 15, 6), 2, false); // Should end on an empty frame
                Play("wobble");

                _parabolaThing = new Parabola_Thing(this, 32, 1);
                shadow = new Shadow(this, new Vector2(0, 0), ShadowType.Normal);
            }

            protected override void AnimationChanged(string name)
            {
                base.AnimationChanged(name);
                if (name.EndsWith('l'))
                {
                    _flip = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    _flip = SpriteEffects.None;
                }
            }
        }

        class Javiera : Entity
        {
            public Javiera(Vector2 position)
            : base(position, "javiera", 16, 16, Drawing.DrawOrder.ENTITIES)
            {
                AddAnimation("walk_d", CreateAnimFrameArray(0, 1), 8);
                AddAnimation("walk_l", CreateAnimFrameArray(4, 5), 8);
                AddAnimation("walk_u", CreateAnimFrameArray(2, 3), 8);
                AddAnimation("walk_r", CreateAnimFrameArray(4, 5), 8);
                AddAnimation("juggle", CreateAnimFrameArray(0, 1), 8);
                AddAnimation("fall", CreateAnimFrameArray(6, 7, 8, 9, 10, 11, 12), 2, false); // Should end on an empty frame
                Play("juggle");
            }

            protected override void AnimationChanged(string name)
            {
                base.AnimationChanged(name);
                if (name.EndsWith('l'))
                {
                    _flip = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    _flip = SpriteEffects.None;
                }
            }
        }

        class ShockWave : Entity
        {
            public ShockWave()
                : base(Vector2.Zero, "shockwave", 16, 16, Drawing.DrawOrder.ENTITIES)
            {
                AddAnimation("move", CreateAnimFrameArray(0, 1, 2, 1), 8); // Remove if we make directional moving shockwaves
                AddAnimation("move_d", CreateAnimFrameArray(0, 1, 2, 1), 8);
                AddAnimation("evaporate", CreateAnimFrameArray(3, 4, 5, 6, 6), 8, false);
                Play("move_d");
            }
        }
    }
}
