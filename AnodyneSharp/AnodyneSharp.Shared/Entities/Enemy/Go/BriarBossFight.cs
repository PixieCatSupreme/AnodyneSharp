using AnodyneSharp.Dialogue;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

                IEnumerator attack = MainAttack(happy, blue);
                while (attack.MoveNext()) yield return null;
            }


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
