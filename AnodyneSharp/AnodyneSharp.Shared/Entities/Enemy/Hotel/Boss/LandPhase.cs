using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Hotel.Boss
{
    [NamedEntity("Eye_Boss", null, 4), Collision(typeof(Player), typeof(Broom), MapCollision = true)]
    public class LandPhase : Entity
    {
        EnemyMarker death_marker = new();

        int health = 6;

        bool broom_hit = false;

        IEnumerator state;
        EntityPool<Bullet> bullets;

        public LandPhase(EntityPreset preset, Player p) : base(preset.Position, "eye_boss_water", 24, 24, Drawing.DrawOrder.ENTITIES)
        {
            if (SoundManager.CurrentSongName != "hotel-boss")
            {
                exists = false;
                return;
            }
            (GlobalState.Map as MapData.Map).IgnoreMusicNextUpdate(); //Make sure music doesn't change back if player moves back up

            AddAnimation("closed", CreateAnimFrameArray(3));
            AddAnimation("walk", CreateAnimFrameArray(4, 5), 6);
            AddAnimation("blink_land", CreateAnimFrameArray(5, 6, 7, 6, 5, 5), 6, false);

            Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + Vector2.One * 16;

            p.grid_entrance = tl + new Vector2(70, 50);

            shadow = new(this, new Vector2(6,-8), ShadowType.Big);
            shadow.visible = false;

            if (preset.Activated)
            {
                Play("walk");
                Position = tl + new Vector2(16, 75);
                state = LandLogic(preset, p, tl);
            }
            else
            {
                state = ToLand(preset, p, tl);
                state.MoveNext(); //initial setup
            }
        }

        IEnumerator ToLand(EntityPreset preset, Player p, Vector2 tl)
        {
            Play("closed");
            Solid = false; //No collision during this part
            Position = tl + new Vector2(44, -32);

            yield return null; //initial setup

            velocity.Y = 8;

            while (
                !(MathUtilities.MoveTo(ref y_push, 22, 40)
                & MathUtilities.MoveTo(ref opacity, 1, 1.8f))
                )
                yield return null;

            while (Position.Y < tl.Y)
                yield return null;

            velocity = Vector2.Zero;
            Play("walk");
            y_push = 0;
            SoundManager.PlaySoundEffect("bubble_triple");

            void shadowPos()
            {
                shadow.SetFrame(offset.Y switch
                {
                    <= 20 => 4,
                    <= 40 => 3,
                    _ => 2,
                });
            }
            shadow.visible = true;
            while (!MathUtilities.MoveTo(ref offset.Y, 90, 90))
            {
                shadowPos();
                yield return null;
            }
            Position = tl + new Vector2(16, 75);

            SoundManager.PlaySoundEffect("fall_1");

            while (!MathUtilities.MoveTo(ref offset.Y, 0, 180))
            {
                shadowPos();
                yield return null;
            }
            shadow.visible = false;

            GlobalState.screenShake.Shake(0.03f, 0.5f);

            preset.Activated = true; //Make sure that if player returns from quick phase 1 revisit boss is already on land
            state = LandLogic(preset, p, tl);
            yield break;
        }

        IEnumerator LandLogic(EntityPreset preset, Player p, Vector2 tl)
        {
            Solid = true;
            height = 11;
            offset.Y = sprite.Height - height;
            Position += offset;

            Vector2 base_pt = Position - Vector2.One * 16;


            yield break;
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
            broom_hit = false;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (!Solid) return;

            if (other is Broom && !_flickering)
            {
                broom_hit = true;
                health--;
                SoundManager.PlaySoundEffect("broom_hit");
                Flicker(1f);
            }
            else if (other is Player p)
            {
                p.ReceiveDamage(1);
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { death_marker };
        }

        private class Bullet : Entity
        {
            public Bullet() : base(Vector2.Zero, "eye_boss_bullet", 16, 16, Drawing.DrawOrder.FG_SPRITES)
            {

            }
        }
    }
}
