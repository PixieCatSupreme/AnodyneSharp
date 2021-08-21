using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Switches
{
    [NamedEntity("Pillar_Switch"), Collision(typeof(Broom), PartOfMap = true)]
    class PillarSwitch : Entity
    {
        float hit_tm = 0;

        public PillarSwitch(EntityPreset preset, Player p) : base(preset.Position, "pillar_switch", 16,16, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("hit", CreateAnimFrameArray(2, 3), 12);
            immovable = true;
            SetFrame(GlobalState.PillarSwitchOn);
        }

        public override void Update()
        {
            base.Update();
            if(hit_tm > 0)
            {
                hit_tm -= GameTimes.DeltaTime;
                if(hit_tm <= 0)
                {
                    SetFrame(GlobalState.PillarSwitchOn);
                }
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
            if(other is Broom && hit_tm <= 0)
            {
                Play("hit");
                hit_tm = 1f;
                SoundManager.PlaySoundEffect("broom_hit");
                GlobalState.PillarSwitchOn = 1 - GlobalState.PillarSwitchOn;
            }
        }
    }
}
