using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Gadget.Switches
{
    [NamedEntity("Switch_Pillar"),Collision(PartOfMap = true)]
    class SwitchPillar : Entity
    {
        int defaultFrame;
        int current;

        const int upFrame = 0;
        const int downFrame = 1;

        public SwitchPillar(EntityPreset preset, Player p) : base(preset.Position,"dame-switch-pillar",16,16,Drawing.DrawOrder.BG_ENTITIES)
        {
            immovable = true;
            defaultFrame = 1 - preset.Frame; //Inverse of what the level editor suggests
            immovable = true;

            AddAnimation("dissolve", CreateAnimFrameArray(upFrame, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, downFrame), 15, false);
            AddAnimation("solidify", CreateAnimFrameArray(downFrame, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, upFrame), 15, false);
            SetFrame(TargetFrame());
            current = TargetFrame();
        }

        /*
         * baseFrame 1 => down by default => GlobalState.PillarSwitchOn ? 0 : 1
         * baseFrame 0 => up by default => GlobalState.PillarSwitchOn ? 1 : 0
         */
        int TargetFrame() => Math.Abs(defaultFrame - GlobalState.PillarSwitchOn);

        public override void Update()
        {
            base.Update();
            if(TargetFrame() != current)
            {
                switch(current)
                {
                    case downFrame:
                        SoundManager.PlaySoundEffect("dash_pad_2");
                        Play("solidify");
                        break;
                    case upFrame:
                        SoundManager.PlaySoundEffect("dash_pad_1");
                        Play("dissolve");
                        break;
                }
                current = TargetFrame();
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if(GetFrame() == upFrame)
            {
                Separate(this, other);
            }
        }
    }
}
