﻿using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Enemy.Crowd;
using AnodyneSharp.Registry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc
{
    [NamedEntity("NPC", type: "generic", 6), Collision(typeof(Player))]
    public class CliffDog : Entity, Interactable
    {
        float timer = 0;

        public CliffDog(EntityPreset preset, Player p) : base(preset.Position, Dog.GetSprite(), Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
            Play("stop");
            facing = Facing.RIGHT;
        }

        public override void Update()
        {
            timer += GameTimes.DeltaTime;
            if(timer > 1)
            {
                if(CurAnimName == "walk")
                {
                    FaceTowards(Position - velocity);
                    velocity = Vector2.Zero;
                    Play("stop");
                    _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
                }
                else if (GlobalState.RNG.NextDouble() > 0.6)
                {
                    velocity = FacingDirection(facing) * 15;
                    Play("walk",newFramerate:8);
                    if(facing == Facing.LEFT)
                    {
                        _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                    }
                }
                timer = 0;
            }
            base.Update();
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("cliff_dog", "top_left");
            return true;
        }
    }
}
