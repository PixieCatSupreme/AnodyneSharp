using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Enemy.Hotel.Boss;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Hotel
{
    [NamedEntity("NPC", "generic", 5)]
    public class EyebossPreview : Entity
    {
        private IEnumerator _stateLogic;

        public static CompositeSpriteRenderer GetSprite() => new(24, 24,
            new StaticSpriteRenderer("sclera", 24, 24, layer: NonEntityLayer.Zero),
            new StaticSpriteRenderer("pupil", 10, 8, layer: new RefLayer(NonEntityLayer.Zero, 1)),
            new AnimatedSpriteRenderer("eyelids", 24, 24, new RefLayer(NonEntityLayer.Zero, 2),
                new Anim("closed", new int[] { 3 }, 1),
                new Anim("blink", new int[] { 0, 1, 2, 3, 2, 1, 0 }, 10, false),
                new Anim("blink_fast", new int[] { 0, 1, 2, 3, 2, 1 }, 20, true),
                new Anim("idle", new int[] { 0 }, 1),
                new Anim("open", new int[] { 3, 2, 1, 0 }, 5, false)
                )
            );

        CompositeSpriteRenderer renderer => sprite as CompositeSpriteRenderer;

        public static readonly Vector2 Eye_Center = new Vector2(7, 8);

        Player player;

        public EyebossPreview(EntityPreset preset, Player p)
            : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;

            renderer.RenderProperties[1].Position = Eye_Center;

            player = p;

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

            Vector2 dir = (player.VisualCenter - VisualCenter);
            dir.Normalize();
            renderer.RenderProperties[1].Position = Eye_Center + dir * 2;

            _stateLogic.MoveNext();
        }

        private IEnumerator StateLogic()
        {
            ISpriteRenderer eyelids = renderer.Renderers[2];
            while(opacity < 1)
            {
                opacity += 0.48f * GameTimes.DeltaTime;
                yield return null;
            }

            eyelids.PlayAnim("open");

            while (!eyelids.AnimFinished)
            {
                yield return null;
            }

            eyelids.PlayAnim("blink");

            while (!eyelids.AnimFinished)
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
