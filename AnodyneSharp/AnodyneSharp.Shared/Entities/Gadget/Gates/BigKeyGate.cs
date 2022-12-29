using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("KeyBlock", null, 1, 2, 3)]
    public class BigKeyGate : BigGate
    {
        Entity keyhole;

        public BigKeyGate(EntityPreset preset, Player p) : base(preset, p)
        {
            //big key gates activate instantly
            _sentinel._maxActivationTime = 0f;

            keyhole = new(Position, "gate_green_slots", 32, 16, new RefLayer(layer_def, 1));
            keyhole.SetFrame(preset.Frame switch
            {
                1 => 2,
                2 => 0,
                3 => 1,
                _ => throw new NotImplementedException()
            });
        }

        IEnumerator<CutsceneEvent> keyAnim()
        {
            Entity _keyAnimSprite = new(_player.Position - new Vector2(0, 16), "key_green", 16, 16, Drawing.DrawOrder.FG_SPRITES);
            _keyAnimSprite.SetFrame(keyhole.Frame * 2);
            _keyAnimSprite.opacity = 0f;

            yield return new EntityEvent(Enumerable.Repeat(_keyAnimSprite, 1));

            while (!MathUtilities.MoveTo(ref _keyAnimSprite.opacity, 1f, 0.6f))
            {
                yield return null;
            }

            _keyAnimSprite.Flicker(0.5f);

            while (!(MathUtilities.MoveTo(ref _keyAnimSprite.Position.X, Position.X + 8, 6f) &
                       MathUtilities.MoveTo(ref _keyAnimSprite.Position.Y, Position.Y, 6f)))
            {
                yield return null;
            }

            SoundManager.PlaySoundEffect("player_jump_down");

            while (!MathUtilities.MoveTo(ref _keyAnimSprite.opacity, 0f, 0.7f))
            {
                yield return null;
            }

            GlobalState.StartCutscene = OpeningSequence();

            yield break;
        }

        public override bool TryUnlock()
        {
            if (GlobalState.inventory.BigKeyStatus[keyhole.Frame])
            {
                GlobalState.StartCutscene = keyAnim();
                return true;
            }
            return false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(Enumerable.Repeat(keyhole,1));
        }

        protected override void BreakLock()
        {
            keyhole.exists = false;
        }
    }
}
