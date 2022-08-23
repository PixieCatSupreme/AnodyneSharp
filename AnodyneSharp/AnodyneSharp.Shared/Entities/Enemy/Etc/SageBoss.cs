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
using SharpDX.Direct2D1;
using static AnodyneSharp.Registry.GlobalState;

namespace AnodyneSharp.Entities.Enemy.Etc
{
    [NamedEntity("Sage_Boss"), Collision(typeof(Player), typeof(Broom), MapCollision = true), Enemy]
    internal class SageBoss : Entity
    {
        private Vector2 _topLeft;

        private EntityPool<ShortAttack> _sBullets;
        private EntityPool<LongAttack> _lBullets;

        private IEnumerator _stateLogic;

        private Player _player;

        private int _health;
        private bool _canHurt;

        public SageBoss(EntityPreset preset, Player p)
            : base(preset.Position, "sage_boss", 16, 24, Drawing.DrawOrder.ENTITIES)
        {
            _topLeft = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);

            AddAnimation("idle", CreateAnimFrameArray(4));
            AddAnimation("idle_d", CreateAnimFrameArray(0, 1, 2, 3), 5, true);
            AddAnimation("dash_d", CreateAnimFrameArray(0, 1, 2, 3), 5, true);
            AddAnimation("dash_u", CreateAnimFrameArray(0, 1, 2, 3), 5, true);
            AddAnimation("walk_u", CreateAnimFrameArray(0, 1, 2, 3), 5, true);
            Play("idle");

            width = height = 10;
            offset.X = 3;
            offset.Y = 8;

            Position.X = _topLeft.X + 72;
            Position += new Vector2(3, 3);

            _player = p;

            _lBullets = new EntityPool<LongAttack>(6, () => new LongAttack());
            _sBullets = new EntityPool<ShortAttack>(6, () => new ShortAttack());

            _stateLogic = Intro();
            immovable = true;

#if DEBUG
            int dVal = 2;
            if (dVal == 1)
            {
                Position.X = _topLeft.X + 80 - width / 2;
                Position.Y = _topLeft.Y + 25;

                SoundManager.PlaySong("sagefight");
                Play("idle_d");

                _stateLogic = Stage1();
            }
            else if (dVal == 2)
            {
                Position.X = _topLeft.X + 80 - width / 2;
                Position.Y = _topLeft.Y + 25;

                SoundManager.PlaySong("sagefight");
                Play("idle_d");

                _stateLogic = Stage2();
            }
#endif
        }

        public override void Update()
        {
            base.Update();

            _stateLogic.MoveNext();
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return _lBullets.Entities.Concat(_sBullets.Entities);
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (!_canHurt)
            {
                return;
            }

            if (other is Player p)
            {
                p.ReceiveDamage(1);
            }
            else if (!_flickering && other is Broom)
            {
                Flicker(1);
                _health--;
                SoundManager.PlaySoundEffect("broom_hit");
            }
        }

        private IEnumerator Intro()
        {
            _canHurt = false;

            facing = Facing.DOWN;
            Play("idle");

            VolumeEvent volumeEvent = new VolumeEvent(0, 0.6f);

            GlobalState.SpawnEntity(volumeEvent);

            while (!volumeEvent.ReachedTarget)
            {
                yield return null;
            }

            while (Vector2.Distance(Position, _player.Position) >= 32 || _player.state != PlayerState.GROUND)
            {
                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("sage", "before_fight");

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            Play("idle_d");
            SoundManager.PlaySoundEffect("teleguy_up");

            visible = false;
            opacity = 0;

            Position.X = _topLeft.X + 80 - width / 2;
            Position.Y = _topLeft.Y + 24;

            while (!MathUtilities.MoveTo(ref opacity, 0.4f, 0.48f))
            {
                yield return null;
            }

            visible = true;

            SoundManager.PlaySoundEffect("teleguy_down");

            while (!MathUtilities.MoveTo(ref opacity, 1f, 0.48f))
            {
                yield return null;
            }

            SoundManager.PlaySong("sagefight");

            GlobalState.flash.Flash(2.18f, Color.Black);
            GlobalState.screenShake.Shake(0.02f, 1.5f);

            while (GlobalState.flash.Active())
            {
                yield return null;
            }

            GlobalState.flash.Flash(2.15f, Color.Black);
            GlobalState.screenShake.Shake(0.025f, 1.5f);

            while (GlobalState.flash.Active())
            {
                Position.X = _topLeft.X + GlobalState.RNG.Next(40, 100);
                Position.Y = _topLeft.Y + GlobalState.RNG.Next(30, 70);

                yield return null;
            }

            Position = _player.Position - new Vector2(2, 16);

            GlobalState.flash.Flash(2.15f, Color.Black);
            GlobalState.screenShake.Shake(0.03f, 1.5f);


            while (GlobalState.flash.Active())
            {
                yield return null;
            }

            GlobalState.flash.Flash(6f, Color.Black);
            GlobalState.screenShake.Shake(0.035f, 1.7f);

            Position.X = _topLeft.X + 80 - width / 2;
            Position.Y = _topLeft.Y + 25;

            opacity = 0;

            while (!MathUtilities.MoveTo(ref opacity, 1f, 0.3f))
            {
                yield return null;
            }

            _stateLogic = Stage1();

            yield break;
        }

        private IEnumerator Stage1()
        {
            int maxHealth = 3;
            _health = maxHealth;
            _canHurt = true;

            float bClock = 0;
            float[] bDelay = new float[] { 1.6f, 1.4f, 1.2f };


            while (_health > 0)
            {
                bClock += GameTimes.DeltaTime;

                int i = maxHealth - _health;
                float s1Vel = 50;

                if (i < 0)
                {
                    i = 0;
                }

                if (bClock > bDelay[i])
                {
                    bClock = 0;
                    _lBullets.Spawn((l) => l.Spawn(_topLeft + new Vector2(48 + (64 - l.width) / 2, 20), new Vector2(0, s1Vel)));
                }

                yield return null;
            }

            foreach (LongAttack lAttack in _lBullets.Entities)
            {
                lAttack.Poof();
            }

            _canHurt = false;

            _stateLogic = Stage2();

            yield break;
        }

        private IEnumerator Stage2()
        {
            int maxHealth = 3;
            _health = maxHealth;
            _canHurt = false;

            bool movedRight = false;

            float[] speed = new float[] { 50, 70, 90 };

            while (_flickering)
            {
                yield return null;
            }

            _lBullets.Spawn((l) => l.SpawnTopBottom(_topLeft + new Vector2(48, 16)));
            _lBullets.Spawn((l) => l.SpawnTopBottom(_topLeft + new Vector2(48, 8 * 16)));

            LongAttack lAttack1 = _lBullets.Entities.OfType<LongAttack>().ElementAt(0);
            LongAttack lAttack2 = _lBullets.Entities.OfType<LongAttack>().ElementAt(1);

            while (lAttack1.IsFlickering)
            {
                yield return null;
            }
            velocity.X = 40;
            immovable = false;

            _canHurt = true;

            _sBullets.Spawn((l) => l.Spawn());
            _sBullets.Spawn((l) => l.Spawn());

            ShortAttack sAttack1 = _sBullets.Entities.OfType<ShortAttack>().ElementAt(0);
            ShortAttack sAttack2 = _sBullets.Entities.OfType<ShortAttack>().ElementAt(1);

            while (_health > 0)
            {
                int i = maxHealth - _health;

                if (i < 0)
                {
                    i = 0;
                }

                sAttack1.Position = Position + new Vector2((width - sAttack1.width) / 2, -10);
                sAttack2.Position = Position + new Vector2((width - sAttack2.width) / 2, 12);

                if (!movedRight && touching.HasFlag(Touching.RIGHT))
                {
                    movedRight = true;
                    velocity.Y = speed[i];

                    yield return null;
                }

                if (touching.HasFlag(Touching.RIGHT))
                {
                    velocity.X = -speed[i];
                }
                else if (touching.HasFlag(Touching.LEFT))
                {
                    velocity.X = speed[i];
                }

                if (touching.HasFlag(Touching.DOWN))
                {
                    velocity.Y = -speed[i];
                }
                else if (touching.HasFlag(Touching.UP))
                {
                    velocity.Y = speed[i];
                }

                yield return null;
            }

            _canHurt = false;

            _stateLogic = Stage3();

            yield break;
        }

        private IEnumerator Stage3()
        {
            yield break;
        }

        [Collision(typeof(Player), typeof(Broom), KeepOnScreen = true)]
        class LongAttack : HurtingEntity
        {
            public bool IsFlickering { get { return _flickering; } }

            public LongAttack()
                : base("sage_fight_long_dust", 64, 16)
            {
                width = 56;
                height = 10;
                offset.X = 4;
                offset.Y = 3;
            }

            public void SpawnTopBottom(Vector2 pos)
            {
                Play("spin");

                Position.X = pos.X + (64 - width) / 2;
                Position.Y = pos.Y + (16 - height) / 2;

                visible = true;
                IsHurting = true;

                Flicker(1);
            }

            public void Spawn(Vector2 pos, Vector2 velocity)
            {
                Play("spin");

                Position = pos;
                this.velocity = velocity;

                visible = true;
                IsHurting = true;
            }

            public override void Update()
            {
                base.Update();

                if (touching != Touching.NONE)
                {
                    Poof();
                }
            }
        }

        [Collision(typeof(Player), typeof(Broom))]
        class ShortAttack : HurtingEntity
        {
            public bool IsFlickering { get { return _flickering; } }

            public ShortAttack()
                : base("sage_attacks", 16, 16)
            {
                AddAnimation("shoot", CreateAnimFrameArray(4, 5), 12);

                width = height = 8;
                offset.X = offset.Y = 4;
            }

            public void Spawn()
            {
                Play("spin");

                visible = true;
                IsHurting = true;
            }
        }

        class HurtingEntity : Entity
        {
            public bool IsHurting { get; set; }

            public HurtingEntity(string texture, int w, int h)
                : base(Vector2.Zero, texture, w, h, Drawing.DrawOrder.ENTITIES)
            {
                AddAnimation("spin", CreateAnimFrameArray(0, 1), 24, true);
                AddAnimation("poof", CreateAnimFrameArray(0, 1, 2), 12, false);

                visible = false;
                IsHurting = false;
            }

            public override void Update()
            {
                base.Update();

                if (CurAnimName == "poof" && _curAnim.Finished)
                {
                    exists = false;
                }
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);

                if (!IsHurting)
                {
                    return;
                }

                if (other is Player p && p.state == PlayerState.GROUND)
                {
                    p.ReceiveDamage(1);
                }
            }

            public void Poof()
            {
                if (CurAnimName != "poof")
                {
                    Play("poof");
                }

                velocity = Vector2.Zero;
                IsHurting = false;
            }
        }
    }
}
