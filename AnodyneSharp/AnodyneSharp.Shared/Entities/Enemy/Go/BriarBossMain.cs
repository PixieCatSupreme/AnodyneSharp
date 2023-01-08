using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.MapData;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Enemy.Go
{
    [NamedEntity("Shadow_Briar", map: "GO")]
    public class BriarBossMain : Entity
    {
        Vector2 tl;
        ThornGate gate;
        VolumeEvent volume;

        BriarBossFight fightStage;
        enum State
        {
            Intro,
            Fight,
            Post
        }
        State state = State.Intro;

        Player player;

        public static AnimatedSpriteRenderer GetSprite() => new("briar", 16, 16,
            new Anim("idle", new int[] { 0 }, 1),
            new Anim("walk_u", new int[] { 4, 5 }, 4),
            new Anim("oh_no", new int[] { 10 }, 1)
            );

        public BriarBossMain(EntityPreset preset, Player p) : base(Vector2.Zero, GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
            Position = tl + new Vector2(80 - width / 2, 32);
            gate = new(tl + Vector2.UnitX * 64);
            volume = new(0, 0.6f);
            player = p;

            if (GlobalState.events.GetEvent("HappyDone") == 0 || GlobalState.events.GetEvent("BlueDone") == 0)
            {
                exists = false;
                volume.exists = false;
            }
        }

        public override void Update()
        {
            base.Update();
            if (GlobalState.ScreenTransition) return;
            switch (state)
            {
                case State.Intro:
                    if (player.Position.Y < tl.Y + 6 * 16)
                    {
                        state = State.Fight;
                        GlobalState.StartCutscene = Intro();
                    }
                    break;
                case State.Fight:
                    if (!fightStage?.exists ?? false)
                    {
                        state = State.Post;
                        gate.exists = false;
                        visible = true;
                        GlobalState.StartCutscene = End();
                    }
                    break;
                case State.Post:
                    player.Position.Y = Math.Min(player.Position.Y, tl.Y + 120);
                    if (Position.Y < tl.Y - 16)
                    {
                        visible = false;
                        velocity = Vector2.Zero;
                    }
                    break;
            }
        }

        IEnumerator<CutsceneEvent> Intro()
        {
            yield return new DialogueEvent(DialogueManager.GetDialogue("briar", "before_fight"));

            SoundManager.PlaySoundEffect("stream");
            SoundManager.StopSong();

            var water_anim = CoroutineUtils.OnceEvery(DoWaterAnim(), 0.2f);
            while (water_anim.MoveNext())
            {
                GlobalState.screenShake.Shake(0.01f, 0.1f);
                yield return null;
            }

            Play("oh_no");
            yield return new DialogueEvent(DialogueManager.GetDialogue("briar", "before_fight"));

            bool flash_active = false;
            GlobalState.flash.Flash(1, Color.White, () => flash_active = true);
            while (!flash_active) yield return null;

            SoundManager.PlaySong("briar-fight");
            visible = false;
            (GlobalState.Map as Map).offset.X = 160 * 2;

            gate.Position.Y += 9 * 16;

            fightStage = new(player);
            GlobalState.SpawnEntity(fightStage);

            yield break;
        }

        IEnumerator<CutsceneEvent> End()
        {
            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 1.5f, 1)) yield return null;
            }

            yield return new DialogueEvent(DialogueManager.GetDialogue("briar", "after_fight"));

            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 1, 1)) yield return null;
            }

            velocity = Vector2.UnitY * -20;
            Play("walk_u");
            SoundManager.PlaySong("go");
            GlobalState.events.BossDefeated.Add(GlobalState.CURRENT_MAP_NAME);

            yield break;
        }

        IEnumerator<string> DoWaterAnim()
        {
            var happy_anim = WaterAnim.DoWaterAnim(tl + new Vector2(0, 16 * 4));
            while (happy_anim.MoveNext())
                yield return null;
            var blue_anim = WaterAnim.DoWaterAnim(tl + new Vector2(16 * 9, 16 * 4));
            while (blue_anim.MoveNext())
                yield return null;

            Point tl_p = GlobalState.Map.ToMapLoc(tl);

            void Set(Point p) => GlobalState.Map.ChangeTile(MapData.Layer.BG, tl_p + p, 194);

            Set(new Point(5, 1));
            yield return null;
            Set(new Point(5, 0));
            yield return null;
            Set(new Point(4, 1));
            yield return null;
            Set(new Point(4, 0));
            yield break;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { gate, volume };
        }
    }

    [Collision(typeof(Player))]
    internal class ThornGate : Entity
    {
        public ThornGate(Vector2 pos) : base(pos, new AnimatedSpriteRenderer("briar_ground_thorn", 32, 16, new Anim("move", new int[] { 6, 7, 8 }, 6)), Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
            (other as Player).ReceiveDamage(1);
        }
    }
}
