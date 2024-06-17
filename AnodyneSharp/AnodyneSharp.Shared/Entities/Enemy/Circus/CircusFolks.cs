using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AnodyneSharp.Entities.Enemy.Circus
{
    [NamedEntity("Circus_Folks", null, 2), Collision(typeof(Player), typeof(Broom), MapCollision = true), Enemy]
    public class CircusFolks : WalkAroundEntity
    {
        public const string DamageDealer = "CircusFolks";
        public const string ArthurDamageDealer = "Arthur";
        public const string JavieraDamageDealer = "Javiera";
        public const string FlameDamageDealer = "CircusFolksFlame";

        private const int walkVel = 90;
        private const float walkTimerMax = 3.0f;
        private const int jumpVel = 100;

        private const int fallTimerMax = 2;
        private EntityPreset _preset;
        private Player _player;
        private Parabola_Thing _parabolaThing;

        private Arthur _arthur;
        private Javiera _javiera;
        private EntityPool<ShockWave> _shockWaves;

        private IEnumerator _stateLogic;

        private Vector2 _topLeft;

        private int health;

        private bool bossRush;

        public static AnimatedSpriteRenderer GetSprite()
        {
            return new("arthur_javiera", 16, 32,
                new Anim("walk_d", new int[] { 0, 1 }, 8),
                new Anim("walk_u", new int[] { 2, 3 }, 12),
                new Anim("walk_r", new int[] { 4, 5 }, 12),
                new Anim("walk_l", new int[] { 4, 5 }, 12),
                new Anim("switch", new int[] { 6, 7 }, 6, false),
                new Anim("throw_d", new int[] { 8, 8 }, 2, false), // The length of all frames minus last is how long the warning lasts.
                new Anim("throw_r", new int[] { 10, 8 }, 2, false),
                new Anim("throw_u", new int[] { 9, 8 }, 2, false),
                new Anim("throw_l", new int[] { 10, 8 }, 2, false),
                new Anim("dying", new int[] { 10, 10, 10 }, 1, false)
                );
        }

        public CircusFolks(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;

            _topLeft = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(16);

            _player.grid_entrance = _topLeft + new Vector2(75, 16 * 7 + 4);

            height = 16;
            offset.Y = 16;

            _arthur = new Arthur(_topLeft + new Vector2(64 - 8 - 16, 0));
            _javiera = new Javiera(_topLeft + new Vector2(64 + 8, 0));

            
            visible = false;

            _parabolaThing = new Parabola_Thing(this, 48, 1);
            shadow = new Shadow(this, new Vector2(0, 0), ShadowType.Normal);
            shadow.visible = false;

            _shockWaves = new EntityPool<ShockWave>(8, () => new ShockWave());

            health = 7;

            _stateLogic = Intro();

            bossRush = preset.TypeValue == "boss_rush";
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { _arthur, _javiera }.Concat(_shockWaves.Entities);
        }

        public override void Update()
        {
            base.Update();

            if (health == 0)
            {
                health = -1;
                _stateLogic = Dying();
            }
            else
            {
                _stateLogic.MoveNext();
            }
        }

        public override void Collided(Entity other)
        {
            if (!visible)
            {
                return;
            }

            if (other is Player p)
            {
                p.ReceiveDamage(1, DamageDealer);
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

            if (!bossRush)
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("circus_folks", "before_fight");

                while (!volumeEvent.ReachedTarget || !GlobalState.LastDialogueFinished)
                {
                    yield return null;
                }
            }
            else
            {
                while (!volumeEvent.ReachedTarget)
                {
                    yield return null;
                }
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

            if (GlobalState.RNG.NextDouble() > 0.6)
            {
                _stateLogic = Jump();
            }
            else
            {
                _stateLogic = Throw();
            }

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

        private IEnumerator Throw()
        {
            velocity = Vector2.Zero;
            Play("switch");

            while (!AnimFinished)
            {
                yield return null;
            }

            bool onFull = false;

            GlobalState.flash.Flash(0.2f, Color.White, () => onFull = true);

            while (!onFull)
            {
                yield return null;
            }

            Facing _javieraFacing = facing;

            FaceTowards(_player.Position);

            PlayFacing("throw");

            while (!AnimFinished)
            {
                yield return null;
            }

            SoundManager.PlaySoundEffect("slasher_atk");

            Flicker(0);
            visible = false;

            _arthur.exists = true;
            _arthur.Flicker(0);

            _arthur.stateLogic = _arthur.BeThrown(Position - new Vector2(0, 16), _player);

            _javiera.exists = true;
            _javiera.Flicker(0);

            _javiera.Position = Position;

            while (_arthur.stateLogic != null)
            {
                yield return null;
            }

            Facing f = GetMovementDirection(_arthur.Position);

            if (_shockWaves.Alive < _shockWaves.Entities.Count() - 1)
            {
                _shockWaves.Spawn(b => b.Spawn(_arthur.Position, f));
                _shockWaves.Spawn(b => b.Spawn(_arthur.Position, FlipFacing(f)));
            }

            _javiera.stateLogic = _javiera.Run(_javieraFacing);

            while (_javiera.stateLogic != null)
            {
                yield return null;
            }

            _arthur.Play("walk_d");

            onFull = false;

            GlobalState.flash.Flash(0.2f, Color.White, () => onFull = true);

            while (!onFull)
            {
                yield return null;
            }

            Position = _javiera.Position;

            health -= _arthur.damage + _javiera.damage;
            if (health < 0) health = 0;

            visible = true;

            _arthur.exists = _javiera.exists = false;

            _arthur.Flicker(0);
            _javiera.Flicker(0);

            facing = _javiera.facing;

            PlayFacing("walk");

            _stateLogic = Walk();

            yield break;
        }

        private IEnumerator Dying()
        {
            SoundManager.StopSong();

            velocity = Vector2.Zero;

            Play("dying");

            SoundManager.PlaySoundEffect("sun_guy_death_short");

            GlobalState.screenShake.Shake(0.04f, 1f);

            foreach (var shockWave in _shockWaves.Entities)
            {
                shockWave.velocity = Vector2.Zero;
                shockWave.Play("evaporate");
            }

            _arthur.Flicker(0);
            _javiera.Flicker(0);

            _arthur.hurts = false;
            _javiera.hurts = false;
            _javiera.layer = DrawOrder.FG_SPRITES;

            if (!bossRush)
            {
                GlobalState.Dialogue = DialogueManager.GetDialogue("circus_folks", "after_fight");

                while (!GlobalState.LastDialogueFinished)
                {
                    yield return null;
                }
            }

            Flicker(0);

            visible = false;

            _arthur.exists = _javiera.exists = true;

            _arthur.Position = Position;
            _javiera.Position = Position - new Vector2(0, 16);

            _arthur.parabolaThing = new Parabola_Thing(_arthur, 24, 1.0f);
            _javiera.parabolaThing = new Parabola_Thing(_javiera, 18, 0.8f);

            _arthur.Play("pre-fall");
            _javiera.Play("pre-fall");

            float fallTimer = 0;

            while (fallTimer <= fallTimerMax)
            {
                fallTimer += GameTimes.DeltaTime;

                float n = (float)Math.Sin(fallTimer * 6.28);

                _arthur.offset.X = -3 * n;
                _javiera.offset.X = 3 * n;

                yield return null;
            }

            SoundManager.PlaySoundEffect("floor_crack");

            _arthur.velocity = Vector2.Normalize(new Vector2(_topLeft.X + 64, _topLeft.Y + 40) - _arthur.Position) * 40;
            _javiera.velocity = Vector2.Normalize(new Vector2(_topLeft.X + 36, _topLeft.Y + 50) - _javiera.Position) * 70;

            while (_arthur.opacity != 0 || _javiera.opacity != 0)
            {
                if (_arthur.parabolaThing.Tick())
                {
                    _arthur.velocity = Vector2.Zero;

                    MathUtilities.MoveTo(ref _arthur.opacity, 0, 0.3f);

                    if (_arthur.CurAnimName != "fall")
                    {
                        _arthur.Play("fall");
                    }
                }

                if (_javiera.parabolaThing.Tick())
                {
                    _javiera.velocity = Vector2.Zero;

                    MathUtilities.MoveTo(ref _javiera.opacity, 0, 0.3f);

                    if (_javiera.CurAnimName != "fall")
                    {
                        _javiera.Play("fall");
                    }
                }

                yield return null;
            }

            bool onFull = false;

            SoundManager.PlaySoundEffect("wb_hit_ground");
            GlobalState.flash.Flash(1, Color.Red, () => onFull = true);

            while (!onFull)
            {
                yield return null;
            }

            _preset.Alive = false;
            exists = _arthur.exists = _javiera.exists = false;

            if (!bossRush)
            {
                GlobalState.events.BossDefeated.Add(GlobalState.CURRENT_MAP_NAME);
                SoundManager.PlaySong("circus");
            }
            else
            {
                SoundManager.PlaySong("bedroom");
            }

            yield break;
        }

        private Facing GetMovementDirection(Vector2 pos)
        {
            Facing facing;

            if (pos.X < _topLeft.X + 16)
            {
                facing = Facing.DOWN;
            }
            else if (pos.X > _topLeft.X + 105)
            {
                facing = Facing.UP;
            }
            else if (pos.Y > _topLeft.Y + 106)
            {
                facing = Facing.RIGHT;
            }
            else
            {
                facing = Facing.LEFT;
            }

            return facing;
        }

        [Collision(typeof(Player), typeof(Broom), MapCollision = true)]
        class Arthur : Entity
        {
            private const int throwVel = 120;

            public int damage;

            public IEnumerator stateLogic;

            public Parabola_Thing parabolaThing;

            public bool hurts;

            public static AnimatedSpriteRenderer GetSprite()
            {
                return new("arthur", 16, 16,
                    new Anim("walk_d", new int[] { 0, 1 }, 8),
                    new Anim("walk_l", new int[] { 4, 5 }, 8),
                    new Anim("walk_u", new int[] { 2, 3 }, 8),
                    new Anim("walk_r", new int[] { 4, 5 }, 8),
                    new Anim("roll", new int[] { 6 }, 6), // For flying through the air
                    new Anim("stunned", new int[] { 8, 9 }, 6),
                    new Anim("wobble", new int[] { 16, 17 }, 8),
                    new Anim("fall_1", new int[] { 10 }, 8),
                    new Anim("fall", new int[] { 10, 11, 12, 13, 14, 15, 6 }, 2, false),
                    new Anim("pre-fall", new int[] { 1 }, 1)
                    );
            }

            public Arthur(Vector2 position)
                : base(position, GetSprite(), DrawOrder.ENTITIES)
            {
                Play("wobble");

                damage = 0;

                hurts = true;
            }

            public override void Update()
            {
                base.Update();

                if (stateLogic != null)
                {
                    if (!stateLogic.MoveNext())
                    {
                        stateLogic = null;
                    }
                }
            }

            public IEnumerator BeThrown(Vector2 position, Player player)
            {
                Position = position;

                visible = true;

                Play("roll");

                damage = 0;

                Vector2 targetPos = player.Position;

                velocity = Vector2.Normalize(targetPos - position) * throwVel;

                /*
                 * Two-step throw because Arthur can be thrown from "inside" the top wall
                 * 
                 * immovable set to avoid Arthur getting pushed out of the tile, making him able to miss the target.
                 * 
                 * As soon as he's close enough(about a tile) from the target, revert back to normal collision check.
                 */
                immovable = true;

                while(Vector2.DistanceSquared(Position,targetPos) > 16*16)
                {
                    yield return null;
                }

                immovable = false;

                while (touching == Touching.NONE)
                {
                    yield return null;
                }

                velocity = Vector2.Zero;

                Play("stunned");

                SoundManager.PlaySoundEffect("hit_ground_1");

                GlobalState.screenShake.Shake(0.03f, 0.1f);

                yield break;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);

                if (!visible || !hurts || stateLogic != null)
                {
                    return;
                }

                if (!_flickering)
                {
                    if (other is Broom)
                    {
                        damage++;
                        Flicker(3);
                    }
                    else if (other is Player p && p.state == PlayerState.GROUND)
                    {
                        p.ReceiveDamage(1, ArthurDamageDealer);
                    }
                }
            }

            public override void Fall(Vector2 fallPoint)
            { }

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

        [Collision(typeof(Player), typeof(Broom), typeof(Arthur), MapCollision = true)]
        class Javiera : WalkAroundEntity
        {
            private const float walkVel = 90 * 1.3f;

            public int damage;

            public IEnumerator stateLogic;

            public Parabola_Thing parabolaThing;

            public bool hurts;

            private bool _touchedArthur;

            public static AnimatedSpriteRenderer GetSprite()
            {
                return new("javiera", 16, 16,
                    new Anim("walk_d", new int[] { 0, 1 }, 8),
                    new Anim("walk_l", new int[] { 4, 5 }, 8),
                    new Anim("walk_u", new int[] { 2, 3 }, 8),
                    new Anim("walk_r", new int[] { 4, 5 }, 8),
                    new Anim("juggle", new int[] { 0, 1 }, 8),
                    new Anim("fall", new int[] { 6, 7, 8, 9, 10, 11, 12 }, 2, false),
                    new Anim("pre-fall", new int[] { 1 },1)
                    );
            }

            public Javiera(Vector2 position)
            : base(position, GetSprite(), Drawing.DrawOrder.ENTITIES)
            {
                Play("juggle");

                damage = 0;

                hurts = true;
            }

            public override void Update()
            {
                base.Update();

                if (stateLogic != null)
                {
                    if (!stateLogic.MoveNext())
                    {
                        stateLogic = null;
                    }
                }
            }

            public IEnumerator Run(Facing direction)
            {
                facing = direction;

                PlayFacing("walk");

                damage = 0;
                _touchedArthur = false;

                while (!_touchedArthur)
                {
                    WalkAboutParameter(walkVel);
                    yield return null;
                }

                velocity = Vector2.Zero;

                yield break;
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);

                if (!visible || !hurts)
                {
                    return;
                }

                if (stateLogic != null && other is Arthur)
                {
                    _touchedArthur = true;
                }
                else if (!_flickering)
                {
                    if (other is Broom)
                    {
                        damage++;
                        Flicker(3);
                    }
                    else if (other is Player p && p.state == PlayerState.GROUND)
                    {
                        p.ReceiveDamage(1, JavieraDamageDealer);
                    }
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
        }

        [Collision(typeof(Player), typeof(Broom), MapCollision = true)]
        class ShockWave : WalkAroundEntity
        {
            private const int swVel = 70;

            public static AnimatedSpriteRenderer GetSprite()
            {
                return new("shockwave", 16, 16, 
                    new Anim("move", new int[] { 0, 1, 2, 1 }, 8),
                    new Anim("evaporate", new int[] { 3, 4, 5, 6, 6 }, 8, false));
            }

            public ShockWave()
                : base(Vector2.Zero, GetSprite(), Drawing.DrawOrder.ENTITIES)
            {
                visible = false;
            }

            public override void Update()
            {
                base.Update();

                if (CurAnimName == "evaporate")
                {
                    if (AnimFinished)
                    {
                        exists = false;
                    }
                }
                else
                {
                    WalkAboutParameter(swVel, false);
                }
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

                bool disappear = false;

                if (other is Player p && p.state == PlayerState.GROUND)
                {
                    p.ReceiveDamage(1, FlameDamageDealer);

                    disappear = true;
                }
                else if (other is Broom)
                {
                    SoundManager.PlaySoundEffect("broom_hit");

                    disappear = true;
                }

                if (disappear)
                {
                    velocity = Vector2.Zero;
                    Play("evaporate");
                }
            }
        }
    }

    public class WalkAroundEntity : Entity
    {
        public WalkAroundEntity(Vector2 pos, AnimatedSpriteRenderer render, DrawOrder layer) : base(pos, render, layer) { }

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

        public override void Fall(Vector2 fallPoint)
        { }
    }
}
