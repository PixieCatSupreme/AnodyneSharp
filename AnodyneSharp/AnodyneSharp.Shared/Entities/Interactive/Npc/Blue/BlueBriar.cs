using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive.Npc.Blue
{
    [NamedEntity("Shadow_Briar", map: "BLUE")]
    internal class BlueBriar : ShadowBriar
    {
        const float appearance_vel = 0.24f;

        IEnumerator<string> state;
        IEnumerator water_anim;

        private BlueMitra _mitra;
        private Player _player;

        public BlueBriar(EntityPreset preset, Player p) 
            : base(preset, p)
        {
            _player = p;

            visible = false;
            state = State();

            Vector2 topLeft = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);

            _mitra = new BlueMitra(new Vector2(topLeft.X + 160, topLeft.Y + 33), this);
        }

        public override void Update()
        {
            base.Update();

            state.MoveNext();

            water_anim?.MoveNext();
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { _mitra };
        }

        IEnumerator<string> State()
        {
            Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);

            while (_player.Position.X >= tl.X + 76 || _player.Position.Y >= tl.Y + 47 || _player.state != PlayerState.GROUND)
            {
                yield return null;
            }

            _mitra.StartEvent();

            while (_mitra.exists)
            {
                yield return null;
            }

            GlobalState.StartCutscene = Entrance();

            yield break;
        }

        IEnumerator<CutsceneEvent> Entrance()
        {
            Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);

            VolumeEvent volume = new(0, 0.6f);
            yield return new EntityEvent(Enumerable.Repeat(volume, 1));

            while (!volume.ReachedTarget)
                yield return null;

            Play("walk_d");
            opacity = 0;
            visible = true;
            Sounds.SoundManager.StopSong();

            while (!MathUtilities.MoveTo(ref opacity, 0.4f, appearance_vel))
                yield return null;

            velocity.Y = 20;
            Sounds.SoundManager.PlaySong("go", 0.4f);
            volume.SetTarget(1f);
            volume.speed = appearance_vel;

            water_anim = CoroutineUtils.OnceEvery(WaterAnim.DoWaterAnim(Position), 0.3f);

            while (MapUtilities.GetInGridPosition(Position).Y < 44)
            {
                MathUtilities.MoveTo(ref opacity, 1f, appearance_vel);
                yield return null;
            }

            opacity = 1f;
            velocity = -20 * Vector2.UnitX;
            Play("walk_l");

            while (MapUtilities.GetInGridPosition(Position).X >= tl.X -16) //wraps around when off screen
                yield return null;

            exists = preset.Alive = false;
            GlobalState.events.IncEvent("BlueDone");

        }

        public class BlueMitra : Entity
        {
            private BlueBriar _parent;

            public static AnimatedSpriteRenderer GetSprite() => new("mitra_bike", 20, 20,
                new Anim("bike_l", new int[] { 2, 3 },8),
                new Anim("bike_r", new int[] { 2, 3 },8),
                new Anim("idle", new int[] { 22 },1)
                );

            public BlueMitra(Vector2 position, BlueBriar parent)
                : base(position, GetSprite(), Drawing.DrawOrder.ENTITIES)
            {
                visible = false;

                _parent = parent;
            }

            public void StartEvent()
            {
                GlobalState.StartCutscene = Event();
            }

            protected override void AnimationChanged(string name)
            {
                base.AnimationChanged(name);
                if (name.EndsWith('l'))
                {
                    _flip = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    _flip = SpriteEffects.None;
                }
            }

            private IEnumerator<CutsceneEvent> Event()
            {
                Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
                visible = true;

                VolumeEvent volume = new(0, 2.4f);
                yield return new EntityEvent(Enumerable.Repeat(volume, 1));

                while (!volume.ReachedTarget)
                {
                    yield return null;
                }

                float timer = 0;

                while (!MathUtilities.MoveTo(ref timer, 0.5f, 0.6f))
                {
                    yield return null;
                }

                Sounds.SoundManager.PlaySong("mitra", 0.5f);

                yield return new DialogueEvent(DialogueManager.GetDialogue("mitra", "one"));

                volume.speed = 0.6f;
                volume.SetTarget(1);

                velocity.X = -70;

                while (Position.X >= tl.X + 75)
                {
                    yield return null;
                }

                GlobalState.PUZZLES_SOLVED += 3;

                velocity.X = 0;

                Play("idle");

                yield return new DialogueEvent(DialogueManager.GetDialogue("mitra", "one"));

                yield return new DialogueEvent(DialogueManager.GetDialogue("mitra", "one"));

                Play("bike_r");

                velocity.X = 40;

                while (Position.X < tl.X + 160)
                {
                    yield return null;
                }

                exists = false;

                yield break;
            }
        }
    }
}
