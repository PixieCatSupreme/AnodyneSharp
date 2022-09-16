using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.GameEvents;
using AnodyneSharp.MapData;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.States;
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
    public class BriarBossFight : Entity
    {
        Entity core;
        BriarBossBody body;
        BlueThorn blue;
        HappyThorn happy;

        IEnumerator state;

        Player player;

        public BriarBossFight(Player p) : base(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid), "briar_overhang", 160, 48, Drawing.DrawOrder.BG_ENTITIES)
        {
            body = new(Position + Vector2.UnitX * 16, p);

            core = new(Position + new Vector2(64, 32), "briar_core", 32, 18, Drawing.DrawOrder.ENTITIES)
            {
                LayerParent = body,
                LayerOffset = 1
            };
            core.AddAnimation("glow", CreateAnimFrameArray(0, 1, 2, 1), 4);
            core.AddAnimation("flash", CreateAnimFrameArray(3, 4), 12);
            core.Play("glow");


            blue = new(Position + new Vector2(5 * 16, 16))
            {
                LayerParent = body,
                LayerOffset = 1
            };
            happy = new(Position + Vector2.One * 16)
            {
                LayerParent = body,
                LayerOffset = 1
            };

            state = StateLogic();
            player = p;
        }

        IEnumerator StateLogic()
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("briar", "before_fight");
            while (!GlobalState.LastDialogueFinished) yield return null;

            while (happy.Health + blue.Health > 0)
            {
                body.State = body.Attack(6 - happy.Health - blue.Health);
                while (body.State is not null) yield return null;

                IEnumerator attack = (happy.Health > 0, blue.Health > 0) switch
                {
                    (false, _) => MainAttack(happy, blue),
                    (_, false) => MainAttack(blue, happy),
                    (true,true) => GlobalState.RNG.Next(2) == 0 ? MainAttack(happy,blue) : MainAttack(blue,happy)
                };

                while (attack.MoveNext()) yield return null;
            }

            GlobalState.StartCutscene = Die();

            yield break;
        }

        IEnumerator MainAttack(BigThorn attacker, BigThorn attacked)
        {
            attacker.Play("active");
            attacked.state = attacked.GetAttacked(attacker, player);
            int health = attacked.Health;
            while(attacked.state is not null)
            {
                if (attacked.Health != health)
                {
                    core.Play("flash");
                    while (attacked.CurAnimName == "hit") yield return null;
                    core.Play("glow");
                    break;
                }
                yield return null;
            }
            while (attacked.state is not null) yield return null;
            attacker.Play("off");
        }

        IEnumerator<CutsceneEvent> Die()
        {
            SoundManager.StopSong();

            EntityPool<HappyThorn.IceExplosion> iceExplosions = new(4, () => new() { layer = Drawing.DrawOrder.FG_SPRITES });
            EntityPool<BlueThorn.DustExplosion> dustExplosions = new(5, () => new() { layer = Drawing.DrawOrder.FG_SPRITES});
            yield return new EntityEvent(iceExplosions.Entities.Concat(dustExplosions.Entities));

            Vector2 randomizePos(Entity target)
            {
                return target.Position + new Vector2(GlobalState.RNG.NextSingle() * target.width, GlobalState.RNG.NextSingle() * target.height);
            }

            for(int i = 0; i < 10; ++i)
            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 0.2f, 1)) yield return null;
                happy.Play("hitloop");
                dustExplosions.Spawn(e => e.Spawn(randomizePos(happy)));
                SoundManager.PlaySoundEffect("hit_wall");
            }
            {
                bool flashed = false;
                GlobalState.flash.Flash(1.5f, new(0xffff1111), () => flashed = true);
                SoundManager.PlaySoundEffect("sun_guy_death_short");
                while (!flashed) yield return null;
                happy.exists = false;
            }

            for (int i = 0; i < 10; ++i)
            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 0.2f, 1)) yield return null;
                blue.Play("hitloop");
                iceExplosions.Spawn(e => e.Spawn(randomizePos(blue)));
                SoundManager.PlaySoundEffect("hit_wall");
            }
            {
                bool flashed = false;
                GlobalState.flash.Flash(1.5f, new(0xff1111ff), () => flashed = true);
                SoundManager.PlaySoundEffect("sun_guy_death_short");
                while (!flashed) yield return null;
                blue.exists = false;
            }

            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 1.5f, 1)) yield return null;
            }

            for (int i = 0; i < 15; ++i)
            {
                float t = 0;
                while (!MathUtilities.MoveTo(ref t, 0.15f, 1)) yield return null;
                core.Play("flash");
                yield return new EntityEvent(Enumerable.Repeat(new Explosion(body) { Position = randomizePos(body) }, 1));
            }
            {
                bool flashed = false;
                GlobalState.flash.Flash(5, Color.White, () => flashed = true);
                SoundManager.PlaySoundEffect("sun_guy_death_long");
                while (!flashed) yield return null;
                body.exists = false;
                core.exists = false;
                exists = false;
                (GlobalState.Map as Map).offset = Vector2.Zero;
                yield return null;
            }

            yield break;
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { body, core, blue, happy };
        }

    }
}
