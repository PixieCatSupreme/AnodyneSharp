using AnodyneSharp.Entities.Base.Rendering;
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

        public static AnimatedSpriteRenderer GetSprite() => new("pillar_switch", 16, 16,
            new Anim("up", new int[] { 0 }, 1),
            new Anim("down", new int[]{1},1),
            new Anim("hit", new int[] { 2, 3 },12)
            );

        public PillarSwitch(EntityPreset preset, Player p) : base(preset.Position, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
            Play(GlobalState.PillarSwitchOn == 0 ? "up" : "down");
        }

        public override void Update()
        {
            base.Update();
            if(hit_tm > 0)
            {
                hit_tm -= GameTimes.DeltaTime;
                if(hit_tm <= 0)
                {
                    Play(GlobalState.PillarSwitchOn == 0 ? "up" : "down");
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
