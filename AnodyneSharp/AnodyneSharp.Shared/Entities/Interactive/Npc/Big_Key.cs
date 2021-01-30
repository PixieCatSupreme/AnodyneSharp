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

namespace AnodyneSharp.Entities.Interactive
{
    [NamedEntity("NPC", "big_key"), Collision(typeof(Player))]
    public class Big_Key : Entity, Interactable
    {
        EntityPreset _preset;
        EntityPool<Sparkle> _sparkles;

        float sparkle_time_max = 0.8f;
        float sparkle_timer = 0f;

        bool activated = false;

        Player _player;
        float radius;

        IEnumerator<string> state;

        public Big_Key(EntityPreset preset, Player p) : base(preset.Position, "key_green", 16,16,Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;
            _player = p;
            SetFrame(preset.Frame);
            immovable = true;
            
            width = 9;
            offset.X = 4;
            Position.X += 4;
            
            shadow = new(this,Vector2.One * 4);
            shadow.visible = false;

            _sparkles = new(3, () => new());

            state = States();
        }

        private IEnumerator<string> States()
        {
            while (!activated)
            {
                yield return "Start";
            }

            shadow.SetFrame(0);
            shadow.visible = true;
            radius = (Position - _player.Center).Length();

            while (!(MathUtilities.MoveTo(ref offset.Y, 16, 6.6f) & MathUtilities.MoveTo(ref radius, 40, 15f)))
            {
                MathUtilities.RotateAround(_player.Center, ref Position, 3f, radius);
                yield return "RotateBig";
            }

            shadow.SetFrame(2);

            while (!(MathUtilities.MoveTo(ref offset.Y, 64, 11f) & MathUtilities.MoveTo(ref radius, 2, 9f)))
            {
                MathUtilities.RotateAround(_player.Center, ref Position, 6.3f, radius);
                yield return "RotateFast";
            }

            while(!MathUtilities.MoveTo(ref offset.Y, 70, 3f))
            {
                yield return "MoveUp";
            }

            while(!MathUtilities.MoveTo(ref offset.Y, 16, 135f))
            {
                yield return "MoveIn";
            }

            GlobalState.flash.Flash(2f, Color.White);
            GlobalState.screenShake.Shake(0.02f, 0.4f);
            SoundManager.PlaySoundEffect("sun_guy_death_long");
            _player.state = PlayerState.GROUND;
            GlobalState.disable_menu = false;
            exists = false;

            yield break;

        }

        public override void Update()
        {
            base.Update();
            sparkle_timer += GameTimes.DeltaTime;
            if(sparkle_timer > sparkle_time_max)
            {
                sparkle_timer = 0f;
                _sparkles.Spawn((s) => s.Spawn(this));
            }
            state.MoveNext();
        }

        public override void Collided(Entity other)
        {
            if(!activated)
                Separate(this, other);
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            activated = true;
            _preset.Alive = false;

            GlobalState.inventory.BigKeyStatus[_curAnim.Frame / 2] = true;
            GlobalState.disable_menu = true;
            _player.state = PlayerState.INTERACT;
            _player.BeIdle();

            return true;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return _sparkles.Entities;
        }

        class Sparkle : Entity
        {
            public Sparkle() : base(Vector2.Zero,"key_sparkle",7,7,Drawing.DrawOrder.FG_SPRITES)
            {
                AddAnimation("sparkle",CreateAnimFrameArray(3,2,1,0),8,false);
            }

            public override void PostUpdate()
            {
                base.PostUpdate();
                if (_curAnim.Finished) exists = false;
            }

            public void Spawn(Big_Key parent)
            {
                Play("sparkle");
                Position = parent.Position - new Vector2(2, 3);
                Position.Y -= parent.offset.Y;
                Position.X += (parent.width + 4) * (float)GlobalState.RNG.NextDouble();
                Position.Y += (parent.height + 6) * (float)GlobalState.RNG.NextDouble();

                if(parent.activated)
                {
                    parent.sparkle_time_max = Math.Max(0.15f, parent.sparkle_time_max - 0.06f);
                    velocity.Y = 20f;
                    SoundManager.PlaySoundEffect("sparkle_1","sparkle_1","sparkle_2","sparkle_2","sparkle_3");
                }
            }
        }
    }
}
