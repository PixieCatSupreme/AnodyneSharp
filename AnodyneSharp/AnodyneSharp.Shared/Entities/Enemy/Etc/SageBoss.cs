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
using static AnodyneSharp.Registry.GlobalState;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Enemy.Etc
{
    [NamedEntity("Sage_Boss"), Collision(typeof(Player), typeof(Broom), MapCollision = true), Enemy]
    internal class SageBoss : Entity
    {
        private Vector2 _topLeft;

        private EntityPool<ShortAttack> _sBullets;
        private EntityPool<LongAttack> _lBullets;

        private IEnumerator _stateLogic;

        private EntityPreset _preset;
        private Player _player;

        private int _health;
        private bool _canHurt;
        private float _flickerLength;

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

            _preset = preset;
            _player = p;

            _lBullets = new EntityPool<LongAttack>(6, () => new LongAttack());
            _sBullets = new EntityPool<ShortAttack>(6, () => new ShortAttack());

            _stateLogic = Intro();
            immovable = true;

#if DEBUG
            int dVal = 0;
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
            else if (dVal == 3)
            {
                _lBullets.Spawn((l) => l.SpawnTopBottom(_topLeft + new Vector2(48, 16)));
                _lBullets.Spawn((l) => l.SpawnTopBottom(_topLeft + new Vector2(48, 8 * 16)));

                SoundManager.PlaySong("sagefight");
                Play("idle_d");

                _stateLogic = Stage3();
            }
            else if (dVal == 4)
            {
                SoundManager.PlaySong("sagefight");
                Play("idle_d");

                _stateLogic = Stage4();
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
                Flicker(_flickerLength);
                _health--;
                SoundManager.PlaySoundEffect("broom_hit");
                flash.Flash(0.4f, Color.Black);
            }
        }

        public override void Fall(Vector2 fallPoint)
        { }

        private IEnumerator Intro()
        {
            _canHurt = false;

            facing = Facing.DOWN;
            Play("idle");

            VolumeEvent volumeEvent = new(0, 0.6f);

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

            _flickerLength = 1;

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

            foreach (LongAttack lAttack in _lBullets.Entities.Cast<LongAttack>())
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

            _flickerLength = 1;

            immovable = false;

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

                SetBulletsPosition(0);

                if (!movedRight && touching.HasFlag(Touching.RIGHT))
                {
                    movedRight = true;
                    velocity.Y = speed[i];

                    yield return null;
                }

                Movement(speed[i], speed[i]);

                yield return null;
            }

            _canHurt = false;
            velocity = Vector2.Zero;

            _stateLogic = Stage3();

            yield break;
        }

        private IEnumerator Stage3()
        {
            int maxHealth = 4;
            _health = maxHealth;
            _canHurt = false;

            _flickerLength = 1;

            immovable = false;

            float[] speed = new float[] { 80, 90, 100, 115 };

            float timer = 0;

            Position.X = _topLeft.X + 80 - width / 2;
            Position.Y = _topLeft.Y + 25;

            while (timer <= 1)
            {
                timer += GameTimes.DeltaTime;

                yield return null;
            }

            timer = 0;

            foreach (var b in _sBullets.Entities)
            {
                b.exists = false;
            }

            int bulletsMade = 0;

            while (bulletsMade < 4)
            {
                timer += GameTimes.DeltaTime;

                if (timer > 0.8)
                {
                    _sBullets.Spawn((s) => s.Spawn());

                    SetBulletsPosition(0);

                    timer = 0;
                    bulletsMade++;
                }

                yield return null;
            }

            velocity.X = 20;
            velocity.Y = speed[0];

            _canHurt = true;

            float r = 0;

            while (_health > 0)
            {
                int i = maxHealth - _health;

                if (i < 0)
                {
                    i = 0;
                }

                timer += GameTimes.DeltaTime;

                if (timer > 2)
                {
                    timer = 0;
                }
                if (timer < 1)
                {
                    r = timer;
                }
                else
                {
                    r = 2 - timer;
                }

                SetBulletsPosition(r * 34);

                Movement(20, speed[i]);

                yield return null;
            }

            _canHurt = false;
            velocity = Vector2.Zero;

            _stateLogic = Stage4();

            yield break;
        }

        private IEnumerator Stage4()
        {
            int maxHealth = 3;
            _health = maxHealth;
            _canHurt = false;

            _flickerLength = 2.5f;

            immovable = false;

            float[] speed = new float[] { 50, 60, 70 };

            float timer = 0;

            Position.X = _topLeft.X + 80 - width / 2;
            Position.Y = _topLeft.Y + 38;

            SoundManager.PlaySoundEffect("teleguy_up");

            foreach (var b in _lBullets.Entities)
            {
                b.exists = false;
            }

            foreach (var b in _sBullets.Entities)
            {
                b.exists = false;
            }

            _canHurt = true;

            while (_health > 0)
            {
                int i = maxHealth - _health;

                if (i < 0)
                {
                    i = 0;
                }

                timer += GameTimes.DeltaTime;

                if (timer > 0.55f)
                {
                    timer = 0;

                    Vector2 pos = _topLeft + new Vector2(4 + 48 + 16 * RNG.Next(0, 4), 16);
                    _sBullets.Spawn((s) => s.Spawn(pos, speed[i]));
                }

                yield return null;
            }

            _canHurt = false;

            StartCutscene =  DyingState();

            yield break;
        }

        private IEnumerator<CutsceneEvent> DyingState()
        {
            foreach (var b in _lBullets.Entities)
            {
                b.exists = false;
            }

            foreach (var b in _sBullets.Entities)
            {
                b.exists = false;
            }

            immovable = true;

            SoundManager.StopSong();
            Play("idle_d");

            yield return new DialogueEvent(DialogueManager.GetDialogue("sage", "after_fight"));

            Play("walk_u");
            velocity.Y = -20;

            while (Position.Y + 16 >= _topLeft.Y)
            {
                yield return null;
            }

            SoundManager.PlaySong("terminal_alt");

            events.BossDefeated.Add("TERMINAL");
            events.SetEvent("SageDied", 1);

            _preset.Alive = false;
            exists = false;

            yield break;
        }

        private void Movement(float xSpeed, float ySpeed)
        {
            if (touching.HasFlag(Touching.RIGHT))
            {
                velocity.X = -xSpeed;
            }
            else if (touching.HasFlag(Touching.LEFT))
            {
                velocity.X = xSpeed;
            }

            if (touching.HasFlag(Touching.DOWN))
            {
                velocity.Y = -ySpeed;
                Play("dash_u");
            }
            else if (touching.HasFlag(Touching.UP))
            {
                velocity.Y = ySpeed;
                Play("dash_d");
            }
        }

        private void SetBulletsPosition(float offset)
        {
            for (int i = 0; i < 4; i++)
            {
                ShortAttack s = _sBullets.Entities.OfType<ShortAttack>().ElementAt(i);

                if (!s.exists)
                {
                    continue;
                }

                switch (i)
                {
                    case 0:
                        s.Position.X = Position.X + (width - s.width) / 2;
                        s.Position.Y = Position.Y - 10 - offset;
                        break;
                    case 1:
                        s.Position.X = Position.X + (width - s.width) / 2;
                        s.Position.Y = Position.Y + 12 + offset;
                        break;
                    case 2:
                        s.Position.X = Position.X - 12 - offset;
                        s.Position.Y = Position.Y;
                        break;
                    case 3:
                        s.Position.X = Position.X + 14 + offset;
                        s.Position.Y = Position.Y;
                        break;
                }
            }
        }

        [Collision(typeof(Player), typeof(Broom), KeepOnScreen = true)]
        class LongAttack : HurtingEntity
        {
            public bool IsFlickering => _flickering;

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

        [Collision(typeof(Player), typeof(Broom), KeepOnScreen = true)]
        class ShortAttack : HurtingEntity
        {
            public bool MapColission { get; set; }
            public bool IsFlickering => _flickering;

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

                MapColission = false;
            }

            public void Spawn(Vector2 pos, float yVel)
            {
                Play("shoot");

                visible = true;
                IsHurting = true;

                Position = pos;

                velocity.Y = yVel;

                MapColission = true;
            }

            public override void Update()
            {
                base.Update();

                if (MapColission && touching != Touching.NONE)
                {
                    Poof();
                }
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

                if (CurAnimName == "poof" && AnimFinished)
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
