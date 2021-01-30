using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity("KeyBlock", null, 1, 2, 3)]
    public class BigKeyGate : BigGate
    {
        Entity _keyAnimSprite;
        bool activated = false;
        IEnumerator<string> _keyAnimState;

        public BigKeyGate(EntityPreset preset, Player p) : base(preset, p)
        {
            //big key gates activate instantly
            _sentinel._maxActivationTime = 0f;

            SetFrame(preset.Frame switch
            {
                1 => 7,
                2 => 5,
                3 => 6,
                _ => 0
            });
            _keyAnimSprite = new Entity(Position, "key_green", 16, 16, Drawing.DrawOrder.FG_SPRITES);
            _keyAnimSprite.SetFrame((_curAnim.Frame - 5) * 2);
            _keyAnimSprite.exists = false;

            _keyAnimState = keyAnim();

            IEnumerator<string> keyAnim()
            {
                while(!activated)
                    yield return "Start";

                _player.state = PlayerState.INTERACT;
                _player.BeIdle();
                _keyAnimSprite.Position = _player.Position - new Vector2(0, 16);
                _keyAnimSprite.exists = true;
                _keyAnimSprite.opacity = 0f;

                while (!MathUtilities.MoveTo(ref _keyAnimSprite.opacity, 1f, 0.6f))
                {
                    yield return "KeyAppearance";
                }

                _keyAnimSprite.Flicker(0.5f);

                while (!(MathUtilities.MoveTo(ref _keyAnimSprite.Position.X, Position.X + 8, 6f) &
                           MathUtilities.MoveTo(ref _keyAnimSprite.Position.Y, Position.Y, 6f)))
                {
                    yield return "MoveToDoor";
                }

                SoundManager.PlaySoundEffect("player_jump_down");

                while(!MathUtilities.MoveTo(ref _keyAnimSprite.opacity, 0f, 0.7f))
                {
                    yield return "IntoKeyhole";
                }

                _state.ChangeState("Open");

                yield break;
            }
        }

        public override void Update()
        {
            base.Update();
            _keyAnimState.MoveNext();
        }

        public override bool TryUnlock()
        {
            if (GlobalState.inventory.BigKeyStatus[_curAnim.Frame - 5])
            {
                activated = true;
                return true;
            }
            return false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(Enumerable.Repeat(_keyAnimSprite, 1));
        }

    }
}
