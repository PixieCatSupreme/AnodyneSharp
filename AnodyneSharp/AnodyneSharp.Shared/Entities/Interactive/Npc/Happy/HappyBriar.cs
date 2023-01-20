using AnodyneSharp.Entities.Events;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc.Happy
{
    [NamedEntity("Shadow_Briar",map:"HAPPY")]
    public class HappyBriar : ShadowBriar
    {
        const float appearance_vel = 0.24f;

        IEnumerator<string> state;
        IEnumerator water_anim;

        public HappyBriar(EntityPreset preset, Player p) : base(preset,p)
        {
            visible = false;
            state = State();
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
        }

        IEnumerator<string> State()
        {
            while (GlobalState.PUZZLES_SOLVED != 1) yield return null;
            GlobalState.StartCutscene = Entrance();
            while(true)
            {
                water_anim?.MoveNext();
                yield return null;
            }
        }

        IEnumerator<CutsceneEvent> Entrance()
        {
            VolumeEvent volume = new(0, 0.6f);
            yield return new EntityEvent(Enumerable.Repeat(volume, 1));
            while (!volume.ReachedTarget)
                yield return null;
            
            Play("walk_d");
            opacity = 0;
            visible = true;
            Sounds.SoundManager.StopSong();

            while(!MathUtilities.MoveTo(ref opacity, 0.4f, appearance_vel))
                yield return null;

            velocity.Y = 20;
            Sounds.SoundManager.PlaySong("go", 0.4f);
            volume.SetTarget(1f);
            volume.speed = appearance_vel;

            water_anim = CoroutineUtils.OnceEvery(WaterAnim.DoWaterAnim(Position), 0.3f);

            while(MapUtilities.GetInGridPosition(Position).Y < 44)
            {
                MathUtilities.MoveTo(ref opacity, 1f, appearance_vel);
                yield return null;
            }

            opacity = 1f;
            velocity = 20 * Vector2.UnitX;
            Play("walk_r");
            while (MapUtilities.GetInGridPosition(Position).X > 20) //wraps around when off screen
                yield return null;

            exists = preset.Alive = false;
            GlobalState.events.IncEvent("HappyDone");

        }
    }
}
