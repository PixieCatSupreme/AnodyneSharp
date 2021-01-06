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
        IState _keyAnimState;

        public BigKeyGate(EntityPreset preset, Player p) : base(preset,p)
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
            _keyAnimSprite = new Entity(Position,"key_green",16,16,Drawing.DrawOrder.FG_SPRITES);
            _keyAnimSprite.SetFrame((_curAnim.Frame - 5) * 2);
            _keyAnimSprite.exists = false;

            _keyAnimState = new StateMachineBuilder()
                .State("KeyAppearance")
                    .Enter((s) =>
                    {
                        _player.state = PlayerState.INTERACT;
                        _player.BeIdle();
                        _keyAnimSprite.Position = _player.Position - new Vector2(0, 16);
                        _keyAnimSprite.exists = true;
                        _keyAnimSprite.opacity = 0f;
                    })
                    .Update((s,t) =>
                    {
                        if(MathUtilities.MoveTo(ref _keyAnimSprite.opacity,1f,0.6f))
                        {
                            _keyAnimState.ChangeState("MoveToDoor");
                        }
                    })
                .End()
                .State("MoveToDoor")
                    .Update((s,t) =>
                    {
                        _keyAnimSprite.Flicker(0.5f);
                        if(MathUtilities.MoveTo(ref _keyAnimSprite.Position.X,Position.X+8,6f) &&
                           MathUtilities.MoveTo(ref _keyAnimSprite.Position.Y,Position.Y,6f))
                        {
                            _keyAnimState.ChangeState("IntoKeyhole");
                            SoundManager.PlaySoundEffect("player_jump_down");
                        }
                    })
                .End()
                .State("IntoKeyhole")
                    .Update((s,t) =>
                    {
                        if(MathUtilities.MoveTo(ref _keyAnimSprite.opacity,0f,0.7f))
                        {
                            _keyAnimState.PopState();
                            _state.ChangeState("Open");
                        }
                    })
                .End()
                .Build();
        }

        public override void Update()
        {
            base.Update();
            _keyAnimState.Update(GameTimes.DeltaTime);
        }

        public override bool TryUnlock()
        {
            if(InventoryManager.BigKeyStatus[_curAnim.Frame - 5])
            {
                _keyAnimState.ChangeState("KeyAppearance");
                return true;
            }
            return false;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return base.SubEntities().Concat(Enumerable.Repeat(_keyAnimSprite,1));
        }

    }
}
