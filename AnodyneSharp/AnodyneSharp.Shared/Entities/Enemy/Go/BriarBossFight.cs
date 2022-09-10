﻿using AnodyneSharp.Dialogue;
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
        }

        IEnumerator StateLogic()
        {
            GlobalState.Dialogue = DialogueManager.GetDialogue("briar", "before_fight");
            while (!GlobalState.LastDialogueFinished) yield return null;

            while (happy.Health + blue.Health > 0)
            {
                body.State = body.Attack(6 - happy.Health - blue.Health);
                while (body.State is not null) yield return null;

                //Choose attacking thorn
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


        class BigThorn : Entity
        {
            public int Health = 3;

            public BigThorn(Vector2 pos, string tex) : base(pos, tex, 64, 80, Drawing.DrawOrder.ENTITIES)
            {
                immovable = true;
                AddAnimation("hit", CreateAnimFrameArray(4, 6, 4, 6, 4, 6, 4, 6, 4, 6, 4, 6), 15);
                AddAnimation("hurt", CreateAnimFrameArray(7, 8), 4);
                AddAnimation("active", CreateAnimFrameArray(4, 5), 5);

                height = 63;
                width = 24;
            }
        }

        class BlueThorn : BigThorn
        {
            public BlueThorn(Vector2 pos) : base(pos, "briar_arm_right")
            {
                AddAnimation("off", CreateAnimFrameArray(1, 2, 3, 0), 4);
                Play("off");
                offset.X = 40;
                Position.X += offset.X;
            }
        }

        class HappyThorn : BigThorn
        {
            public HappyThorn(Vector2 pos) : base(pos, "briar_arm_left")
            {
                AddAnimation("off", CreateAnimFrameArray(0, 1, 2, 3), 4);
                Play("off");
            }

        }
    }
}
