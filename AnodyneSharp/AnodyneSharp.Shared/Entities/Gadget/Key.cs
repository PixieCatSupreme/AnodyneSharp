using System;
using System.Collections.Generic;
using System.Text;
using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;

namespace AnodyneSharp.Entities.Gadget
{
    [NamedEntity, Collision(typeof(Player), typeof(Broom))]
    public class Key : Entity
    {
        EntityPreset _preset;

        bool bossRush;

        public Key(EntityPreset preset, Player p)
            : base(preset.Position, new StaticSpriteRenderer("key", 16, 16, preset.Frame), DrawOrder.ENTITIES)
        {

            _preset = preset;

            if (preset.TypeValue == "boss_rush")
            {
                bossRush = true;

                visible = false;

                if (GlobalState.ENEMIES_KILLED > 1)
                {
                    visible = true;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (visible == false && bossRush && GlobalState.ENEMIES_KILLED > 0)
            {
                visible = true;
            }
        }

        public override void Collided(Entity other)
        {
            if (visible)
            {
                visible = false;
                GlobalState.inventory.AddCurrentMapKey();
                SoundManager.PlaySoundEffect("keyget");

                if (_preset != null)
                {
                    _preset.Alive = false;
                    exists = false;
                }
            }
        }
    }
}
