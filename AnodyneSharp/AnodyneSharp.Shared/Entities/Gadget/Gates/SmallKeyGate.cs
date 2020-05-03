﻿using AnodyneSharp.Drawing;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget.Gates
{
    public class SmallKeyGate : Gate
    {
        public SmallKeyGate(Vector2 pos) : base(pos, "keyhole", 16, 16, DrawOrder.ENTITIES)
        {
            SetFrame(0);
            AddAnimation("Open", CreateAnimFrameArray(16, 17, 18, 19, 20), 10, false);
        }

        public override bool TryUnlock()
        {
            if (InventoryState.GetCurrentMapKeys() > 0)
            {
                InventoryState.RemoveCurrentMapKey();
                SoundManager.PlaySoundEffect("unlock");
                solid = false;
                return true;
            }
            else
            {
                //TODO play keyblock dialogue
                return false;
            }
        }
    }
}