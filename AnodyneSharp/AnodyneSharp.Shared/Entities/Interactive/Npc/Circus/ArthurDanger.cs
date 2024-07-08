using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Circus
{
    [NamedEntity("Circus_Folks", "danger", 0), Collision(typeof(Dust))]
    public class ArthurDanger : Entity
    {
        private const float HurtTimerMax = 1.3f;
        private const float WaitTimerMax = 1f;

        private float _hurtTimer;
        private float _waitTimer;

        private Vector2 _initPos;

        EntityPreset _preset;
        private Parabola_Thing _parabola;
        private Dust _dustPillow;

        private IEnumerator _stateLogic;

        private bool _collided;

        public static AnimatedSpriteRenderer GetSprite() => new("arthur", 16, 16,
            new Anim("wobble", new int[] { 16, 17 }, 8),
            new Anim("walk_d", new int[] { 0, 1 }, 8),
            new Anim("walk_l", new int[] { 4, 5 }, 8),
            new Anim("walk_u", new int[] { 2, 3 }, 8),
            new Anim("walk_r", new int[] { 4, 5 }, 8),
            new Anim("roll", new int[] { 6 }, 6), // For flying through the air
            new Anim("stunned", new int[] { 8, 9 }, 6),
            new Anim("fall_1", new int[] { 10 }, 8),
            new Anim("fall", new int[] { 10, 11, 12, 13, 14, 15, 6 }, 2, false) // Should end on an empty frame
            );

        public ArthurDanger(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;

            _parabola = new Parabola_Thing(this, 32, 1);

            shadow = new Shadow(this, new Vector2(0, -2), ShadowType.Normal);

            Position.Y -= 32;
            offset.Y = 5 * 16;

            _initPos = Position;

            _dustPillow = new Dust(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(46, 16), p);

            _stateLogic = StateLogic();
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

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { _dustPillow };
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            _collided = true;
        }

        private IEnumerator StateLogic()
        {
            while (GlobalState.ScreenTransition)
            {
                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("arthur", "alone", 0);

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            while (!(_collided && offset.X < 0.5f))
            {
                _hurtTimer += GameTimes.DeltaTime;

                offset.X = shadow.offset.X = -3 * (float)Math.Sin((_hurtTimer / HurtTimerMax) * 6.28);

                yield return null;
            }

            offset.X = shadow.offset.X = 0;
            Play("fall_1");

            SoundManager.PlaySoundEffect("fall_1");

            while (offset.Y > 0)
            {
                offset.Y -= GameTimes.DeltaTime * 30 * (1 + 3 * ((80 - offset.Y) / 80));

                shadow.UpdateShadow(offset.Y / 80);

                yield return null;
            }

            _dustPillow.Play("poof");
            SoundManager.PlaySoundEffect("dustpoof");
            Play("stunned");

            GlobalState.PUZZLES_SOLVED++;

            GlobalState.screenShake.Shake(0.01f, 0.2f);
            shadow.visible = false;

            while (_waitTimer < WaitTimerMax)
            {
                _waitTimer += GameTimes.DeltaTime;

                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("arthur", "alone", 1);

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            Play("walk_d");

            while (!MathUtilities.MoveTo(ref Position.Y, _initPos.Y + 32, 60))
            {
                yield return null;
            }

            Play("walk_l");

            while (!_parabola.Tick())
            {
                yield return null;
            }

            while (!MathUtilities.MoveTo(ref Position.X, _initPos.X - 40, 60))
            {
                yield return null;
            }

            _preset.Alive = false;
            exists = false;

            yield break;
        }
    }
}