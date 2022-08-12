using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Entities.Enemy.Circus
{
    [NamedEntity("Circus_Folks", null, 2), Collision(typeof(Player), typeof(Broom), MapCollision = true)]
    internal class CircusFolks : WalkAroundEntity
    {
        private const int walkVel = 90;
        private const float walkTimerMax = 3.0f;
        private const int jumpVel = 100;

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

        public override void Fall(Vector2 fallPoint)
        { }

        public override void Collided(Entity other)
        {
            if (!visible)
            {
                return;
            }

            if (other is Player p)
            {
                p.ReceiveDamage(1);
            }
            else if (!_flickering && other is Broom)
            {
                health--;
                Flicker(1);
                SoundManager.PlaySoundEffect("broom_hit");
            }
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

            Position = _topLeft + new Vector2(56, 0);

            _stateLogic = Walk();

            yield break;
        }

        private IEnumerator Walk()
        {
            float walkTimer = 0f;

            while (walkTimer <= walkTimerMax)
            {
                walkTimer += GameTimes.DeltaTime;

                WalkAboutParameter(walkVel);
                yield return null;
            }

            //TODO random between throw and jump
            _stateLogic = Jump();

            yield break;
        }

        private IEnumerator Jump()
        {
            Vector2 target = _topLeft;
            target.X += _player.Position.X > _topLeft.X + 60 ? 0 : 112;
            target.Y += _player.Position.Y > _topLeft.Y + 60 ? 0 : 112;


            float distance = Vector2.Distance(Position, target);

            velocity = (target - Position) / distance * jumpVel;

            _parabolaThing = new Parabola_Thing(this, 48, distance / jumpVel);
            //shadow.Position += new Vector2(4, 8);

            shadow.visible = true;

            SoundManager.PlaySoundEffect("player_jump_up");

            while (!_parabolaThing.Tick())
            {
                yield return null;
            }

            shadow.visible = false;

            SoundManager.PlaySoundEffect("player_jump_down");
            SoundManager.PlaySoundEffect("wb_tap_ground");

            velocity = Vector2.Zero;
            Position = target;

            //Sets the new direction after the jump
            if (target == _topLeft)
            {
                facing = Facing.DOWN;
            }
            else if (target.X > _topLeft.X && target.Y == _topLeft.Y)
            {
                facing = Facing.LEFT;
            }
            else if (target.X > _topLeft.X && target.Y > _topLeft.Y)
            {
                facing = Facing.UP;
            }
            else
            {
                facing = Facing.RIGHT;
            }

            if (_shockWaves.Alive <= 5)
            {
                _shockWaves.Spawn(b => b.Spawn(Position, Facing.LEFT));
                _shockWaves.Spawn(b => b.Spawn(Position, Facing.RIGHT));
            }

            _stateLogic = Walk();

            yield break;
        }

        class Arthur : Entity
        {
            public int damage;

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

                damage = 0;
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

        class Javiera : WalkAroundEntity
        {
            public int damage;

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

                damage = 0;
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

        [Collision(typeof(Player), typeof(Broom), MapCollision = true)]
        class ShockWave : WalkAroundEntity
        {
            private const int swVel = 70;

            public ShockWave()
                : base(Vector2.Zero, "shockwave", 16, 16, Drawing.DrawOrder.ENTITIES)
            {
                AddAnimation("move", CreateAnimFrameArray(0, 1, 2, 1), 8);
                AddAnimation("evaporate", CreateAnimFrameArray(3, 4, 5, 6, 6), 8, false);

                visible = false;
            }

            public override void Update()
            {
                base.Update();

                if (CurAnimName == "evaporate" && _curAnim.Finished)
                {
                    exists = false;
                }

                WalkAboutParameter(swVel, false);
            }

            public void Spawn(Vector2 pos, Facing direction)
            {
                Position = pos;
                velocity = FacingDirection(direction) * swVel;

                facing = direction;

                Play("move");

                visible = true;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);

                if (CurAnimName == "evaporate")
                {
                    return;
                }

                if (other is Player p)
                {
                    p.ReceiveDamage(1);
                }
                else if (other is Broom)
                {
                    SoundManager.PlaySoundEffect("broom_hit");
                }

                velocity = Vector2.Zero;
                Play("evaporate");
            }
        }
    }
    class WalkAroundEntity : Entity
    {
        public WalkAroundEntity(Vector2 pos, string textureName, int frameWidth, int frameHeight, DrawOrder layer)
            : base(pos, textureName, frameWidth, frameHeight, layer)
        {
        }

        protected void WalkAboutParameter(float speed, bool playAnim = true)
        {
            Vector2 center = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + Vector2.One * 80;

            facing = touching switch
            {
                Touching.LEFT => Position.Y > center.Y ? Facing.UP : Facing.DOWN,
                Touching.RIGHT => Position.Y > center.Y ? Facing.UP : Facing.DOWN,
                Touching.UP => Position.X > center.X ? Facing.LEFT : Facing.RIGHT,
                Touching.DOWN => Position.X > center.X ? Facing.LEFT : Facing.RIGHT,
                _ => facing,
            };

            if (playAnim)
            {
                PlayFacing("walk");
            }

            velocity = FacingDirection(facing) * speed;
        }
    }
}
