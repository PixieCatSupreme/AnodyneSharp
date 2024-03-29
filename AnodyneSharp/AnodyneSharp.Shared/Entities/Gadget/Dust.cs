﻿using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities
{
    public record DustFallEvent : GameEvents.GameEvent { }

    [NamedEntity, Collision(typeof(Dust), MapCollision = true, KeepOnScreen = true)]
    public class Dust : Entity
    {
        public Broom b; //used to un-unpoof on dust-dust collision

        public bool ON_CONVEYOR = false;
        public bool IS_RAFT = false;

        public static AnimatedSpriteRenderer GetSprite() => new("dust", 16, 16,
            new Anim("poofed", new int[] { 0 },1),
            new Anim("unpoofed", new int[] { 4 },1),
            new Anim("poof", new int[] { 0, 1, 2, 3, 4 },13,false),
            new Anim("fallpoof", new int[] { 0, 1, 2, 3, 4 },13,false),
            new Anim("unpoof", new int[] { 3, 2, 1, 0 },13,false)
            );

        public Dust(EntityPreset preset, Player p) : this(preset.Position, p)
        {
        }

        public Dust(Vector2 position, Player p) : base(position, GetSprite(), Drawing.DrawOrder.BG_ENTITIES)
        {
            b = p?.broom;
        }

        public Dust() : this(Vector2.Zero,null)
        {
            exists = false;
        }

        public override void Collided(Entity other)
        {
            if(!AnimFinished && CurAnimName == "unpoof" && b?.dust == this)
            {
                exists = false;
                b.just_released_dust = false;
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            velocity = Vector2.Zero;
            ON_CONVEYOR = false;
            if(AnimFinished && (CurAnimName == "fallpoof" || CurAnimName == "poof"))
            {
                exists = false;
                if(CurAnimName == "fallpoof")
                {
                    GlobalState.FireEvent(new DustFallEvent());
                }
            }
        }

        public override void Fall(Vector2 fallPoint)
        {
            if (IS_RAFT) return;

            if(CurAnimName == "unpoof")
            {
                if(AnimFinished)
                {
                    Play("fallpoof");
                }
                return;
            }
        }

        public override void Conveyor(Touching direction)
        {
            if (direction != Touching.ANY && !IS_RAFT)
            {
                velocity = FacingDirection(FacingFromTouching(direction)) * 10;
            }
            ON_CONVEYOR = true;
        }
    }
}
