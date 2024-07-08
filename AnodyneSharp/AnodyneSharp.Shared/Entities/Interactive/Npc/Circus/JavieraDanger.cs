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
    [NamedEntity("Circus_Folks", "danger", 1)]
    public class JavieraDanger : Entity
    {
        private const float WaitTimerMax = 1f;

        private float _waitTimer;

        private Vector2 _initPos;

        EntityPreset _preset;

        private IEnumerator _stateLogic;

        public static AnimatedSpriteRenderer GetSprite() => new("javiera_juggle", 16, 24,
            new Anim("juggle", new int[] { 0, 1 }, 8),
            new Anim("walk_d", new int[] { 0, 1 }, 8),
            new Anim("walk_l", new int[] { 4, 5 }, 8),
            new Anim("walk_u", new int[] { 2, 3 }, 8),
            new Anim("walk_r", new int[] { 4, 5 }, 8),
            new Anim("fall", new int[] { 6, 7, 8, 9, 10, 11, 12 }, 2, false) // Should end on an empty frame
            );

        public JavieraDanger(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;

            _initPos = Position;

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

        private IEnumerator StateLogic()
        {
            while (GlobalState.ScreenTransition)
            {
                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("javiera", "alone", 0);

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            SetTexture("javiera", 16, 16);

            Play("walk_d");

            while (GlobalState.ENEMIES_KILLED < 2)
            {
                yield return null;
            }

            while (_waitTimer < WaitTimerMax)
            {
                _waitTimer += GameTimes.DeltaTime;

                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("javiera", "alone", 1);

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            while (!MathUtilities.MoveTo(ref Position.Y, _initPos.Y, 60))
            {
                yield return null;
            }

            Play("walk_l");

            while (!MathUtilities.MoveTo(ref Position.X, _initPos.X - 66, 60))
            {
                yield return null;
            }

            _preset.Alive = false;
            exists = false;

            GlobalState.PUZZLES_SOLVED++;

            yield break;
        }
    }
}