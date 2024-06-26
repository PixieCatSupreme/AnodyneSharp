﻿using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Logging;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive
{
    [NamedEntity("NPC", "big_key"), Collision(typeof(Player))]
    public class Big_Key : Entity, Interactable
    {
        EntityPreset _preset;
        EntityPool<Sparkle> _sparkles;

        float sparkle_time_max = 0.8f;
        float sparkle_timer = 0f;

        Player _player;
        float radius;

        public Big_Key(EntityPreset preset, Player p) : base(preset.Position, new StaticSpriteRenderer("key_green", 16, 16, preset.Frame), Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;
            immovable = true;

            width = 9;
            offset.X = 4;
            Position.X += 4;

            shadow = new(this, Vector2.One * 4)
            {
                visible = false
            };

            _sparkles = new(3, () => new());
        }

        private IEnumerator<CutsceneEvent> States()
        {
            shadow.SetFrame(0);
            shadow.visible = true;
            radius = (Position - _player.Center).Length();

            while (!(MathUtilities.MoveTo(ref offset.Y, 16, 18) & MathUtilities.MoveTo(ref radius, 40, 12)))
            {
                MathUtilities.RotateAround(_player.Center, ref Position, 3.6f, radius);
                yield return null;
            }

            shadow.SetFrame(2);

            while (!(MathUtilities.MoveTo(ref offset.Y, 64, 18) & MathUtilities.MoveTo(ref radius, 2, 8.4f)))
            {
                MathUtilities.RotateAround(_player.Center, ref Position, 6.2f, radius);
                yield return null;
            }

            while (!MathUtilities.MoveTo(ref offset.Y, 70, 3))
            {
                yield return null;
            }

            while (!MathUtilities.MoveTo(ref offset.Y, 16, 132))
            {
                yield return null;
            }

            GlobalState.flash.Flash(2f, Color.White);
            GlobalState.screenShake.Shake(0.02f, 0.4f);
            SoundManager.PlaySoundEffect("sun_guy_death_long");
            exists = false;

            yield break;

        }

        public override void Update()
        {
            base.Update();
            sparkle_timer += GameTimes.DeltaTime;
            if (sparkle_timer > sparkle_time_max)
            {
                sparkle_timer = 0f;
                _sparkles.Spawn((s) => s.Spawn(this,!Solid));
            }
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            _preset.Alive = false;
            Solid = false;

            GlobalState.inventory.BigKeyStatus[Frame / 2] = true;
            GlobalState.StartCutscene = States();

            return true;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return _sparkles.Entities;
        }

        class Sparkle : Entity
        {
            public Sparkle() : base(Vector2.Zero, new AnimatedSpriteRenderer("key_sparkle", 7, 7, new Anim("sparkle",new int[] { 3, 2, 1, 0 },8,false)), Drawing.DrawOrder.FG_SPRITES)
            {
            }

            public override void PostUpdate()
            {
                base.PostUpdate();
                if (AnimFinished) exists = false;
            }

            public void Spawn(Big_Key parent, bool make_sound)
            {
                Play("sparkle");
                Position = parent.Position - new Vector2(2, 3);
                Position.Y -= parent.offset.Y;
                Position.X += (parent.width + 4) * (float)GlobalState.RNG.NextDouble();
                Position.Y += (parent.height + 6) * (float)GlobalState.RNG.NextDouble();

                if (make_sound)
                {
                    parent.sparkle_time_max = Math.Max(0.15f, parent.sparkle_time_max - 0.06f);
                    velocity.Y = 20f;
                    SoundManager.PlaySoundEffect("sparkle_1", "sparkle_1", "sparkle_2", "sparkle_2", "sparkle_3");
                }
            }
        }
    }
}
