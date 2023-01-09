using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Gadget
{
    public abstract class BigGate : KeyCardGate
    {
        protected Player _player;

        public static AnimatedSpriteRenderer GetSprite() => new("gate_green", 32, 16,
            new Anim("0", new int[] { 0 },1),
            new Anim("1", new int[] { 1 }, 1),
            new Anim("2", new int[] { 2 }, 1),
            new Anim("3", new int[] { 3 }, 1),
            new Anim("4", new int[] { 4 }, 1)
            );

        public BigGate(EntityPreset preset, Player p) : base(preset, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            _sentinel.Position = Position + new Vector2(11, -3);
            _sentinel.width = 10;
            _player = p;
        }

        protected IEnumerator<CutsceneEvent> OpeningSequence()
        {
            float t = 0;
            for (int i = 0; i <= 4; ++i)
            {
                while (!MathUtilities.MoveTo(ref t, 0.8f, 1f)) yield return null;
                t = 0;

                GlobalState.screenShake.Shake(0.02f, 0.3f);
                SoundManager.PlaySoundEffect("hit_ground_1");
                if (i == 0) BreakLock();
                Play(i.ToString());
            }
            while (!MathUtilities.MoveTo(ref t, 0.8f, 1f)) yield return null;
            SoundManager.PlaySoundEffect("open");
            exists = false;

            yield break;
        }

        protected abstract void BreakLock();
    }
}
