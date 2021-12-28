using AnodyneSharp.Registry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Hotel
{
    [NamedEntity("NPC", "generic", 5)]
    class EyebossPreview : Entity
    {
        private IEnumerator _stateLogic;

        public EyebossPreview(EntityPreset preset, Player p)
            : base(preset.Position, "eye_boss_water", 24, 24, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("blink", CreateAnimFrameArray( 0, 1, 2, 3, 2, 1, 0), 10, false);
            AddAnimation("open", CreateAnimFrameArray(3, 2, 1, 0), 5, false);
            AddAnimation("closed", CreateAnimFrameArray(3));
            Play("closed");

            immovable = true;

            opacity = 0;

            if (GlobalState.events.BossDefeated.Contains("HOTEL") || (GlobalState.events.GetEvent("EyebossPreviewPlayed")> 0 && GlobalState.RNG.NextDouble() > 0.3))
            {
                exists = false;
            }
            else
            {
                GlobalState.events.IncEvent("EyebossPreviewPlayed");
            }

            _stateLogic = StateLogic();
        }

        public override void Update()
        {
            base.Update();

            _stateLogic.MoveNext();
        }

        private IEnumerator StateLogic()
        {
            while(opacity < 1)
            {
                opacity += 0.48f * GameTimes.DeltaTime;
                yield return null;
            }

            Play("open");

            while (!_curAnim.Finished)
            {
                yield return null;
            }

            Play("blink");

            while (!_curAnim.Finished)
            {
                yield return null;
            }

            while (opacity > 0)
            {
                opacity -= 3.0f * GameTimes.DeltaTime;
                yield return null;
            }

            exists = false;

            yield break;
        }
    }
}
