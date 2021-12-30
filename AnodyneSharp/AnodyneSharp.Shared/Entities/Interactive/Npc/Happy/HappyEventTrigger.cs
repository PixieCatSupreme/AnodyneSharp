using AnodyneSharp.Entities.Events;
using AnodyneSharp.MapData;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Happy
{
    [NamedEntity("NPC","generic",1)]
    public class HappyEventTrigger : Entity
    {
        Player _p;
        IEnumerator<string> _state;
        VolumeEvent volume;

        public HappyEventTrigger(EntityPreset preset, Player p) : base(preset.Position,Drawing.DrawOrder.BACKGROUND)
        {
            visible = false;
            if(GlobalState.events.GetEvent("HappyStarted") != 0)
            {
                exists = false;
            }
            _p = p;
            _state = StateLogic();
            volume = new(0, 0.9f);
        }

        public override void Update()
        {
            base.Update();
            _state.MoveNext();
        }

        IEnumerator<string> StateLogic()
        {
            while(MapUtilities.GetInGridPosition(_p.Position).X > 80)
            {
                volume.Update();
                yield return "Waiting";
            }
            Sounds.SoundManager.StopSong();
            var sound = Sounds.SoundManager.PlaySoundEffect("red_cave_rise");
            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("happy_npc", "briar");
            while(!GlobalState.LastDialogueFinished || sound.State == Microsoft.Xna.Framework.Audio.SoundState.Playing)
            {
                GlobalState.screenShake.Shake(0.02f, 0.1f);
                _p.dontMove = true;
                yield return "Dialogue";
            }
            _p.dontMove = false;
            GlobalState.events.IncEvent("HappyStarted");

            GlobalState.flash.Flash(1f, Color.Red, () => {
                ((Map)GlobalState.Map).ReloadSettings(_p);
                GlobalState.darkness.ForceAlpha(1);
            });

            yield break;
        }
    }
}
