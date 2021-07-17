using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Redcave
{
    [NamedEntity, Enemy, Collision(typeof(Player), typeof(Broom), MapCollision = true)]
    public class Red_Boss : Entity
    {
        IEnumerator state;
        Player player;
        EntityPreset preset;
        Ripple ripple;

        int health = 12;
        float invincible_timer = 0f;

        const float push_tick_max = 0.15f;

        public Red_Boss(EntityPreset preset, Player p) : base(preset.Position, "red_boss", 32, 32, Drawing.DrawOrder.ENTITIES)
        {
            height = 19;
            width = 26;
            offset = new Vector2(3, 13);

            AddAnimation("bob", CreateAnimFrameArray(0), 20);
            AddAnimation("close_eyes", CreateAnimFrameArray(1), 10, false);
            AddAnimation("warn", CreateAnimFrameArray(2), 24);
            AddAnimation("die", CreateAnimFrameArray(0, 1, 2, 1), 3, false);

            Play("close_eyes");

            ripple = new(this);

            immovable = true;

            player = p;
            this.preset = preset;

            state = State();

        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { ripple };
        }

        public override void Update()
        {
            base.Update();
            invincible_timer -= GameTimes.DeltaTime;
            state.MoveNext();
        }

        public override void Collided(Entity other)
        {
            if (health == 0) return;

            if(other is Player p)
            {
                p.ReceiveDamage(1);
            }
            else if(other is Broom && invincible_timer < 0)
            {
                Flicker(0.5f);
                invincible_timer = 1.3f;
                health--;
                SoundManager.PlaySoundEffect("redboss_moan");
            }
        }

        IEnumerator State()
        {
            y_push = sprite.Height;
            player.grid_entrance = MapUtilities.GetRoomUpperLeftPos(MapUtilities.GetRoomCoordinate(Position)) + Vector2.One * 20;

            GlobalState.SpawnEntity(new VolumeEvent(0, 3));

            while(MapUtilities.GetInGridPosition(player.Position).X < 48)
            {
                yield return null;
            }

            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("redboss", "before_fight");

            float push_timer = 0f;
            while(!GlobalState.LastDialogueFinished)
            {
                SoundManager.PlaySoundEffect("bubble_loop"); //Call each frame to get looping behavior out of a sound effect
                push_timer += GameTimes.DeltaTime;
                if(push_timer >= push_tick_max)
                {
                    push_timer = 0f;
                    if(y_push > 0)
                    {
                        GlobalState.screenShake.Shake(0.021f, 0.1f);
                        y_push--;
                    }
                }
                yield return null;
            }

            SoundManager.PlaySong("redcave-boss");
            Play("bob");

            while(health > 0)
            {
                //TODO: phases logic
                yield return null;
            }

            //Kill all subentities

            SoundManager.StopSong();
            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("redboss", "after_fight");
            GlobalState.screenShake.Shake(0.05f, 0.1f);
            GlobalState.flash.Flash(1f, Color.Red);

            while(!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            Play("die");
            SoundManager.PlaySoundEffect("redboss_death");
            GlobalState.wave.active = true;

            y_push = 0;

            while(y_push < sprite.Height)
            {
                MathUtilities.MoveTo(ref ripple.opacity, 0, 0.3f);

                push_timer += GameTimes.DeltaTime;
                if (push_timer >= push_tick_max)
                {
                    push_timer = 0f;
                    y_push++;
                }
                yield return null;
            }

            float final_timer = 2f;
            while(final_timer > 0f)
            {
                final_timer -= GameTimes.DeltaTime;
                yield return null;
            }

            preset.Alive = exists = false;
            GlobalState.wave.active = false;
            SoundManager.PlaySong("redcave");
            GlobalState.events.BossDefeated.Add("REDCAVE");
            yield break;
        }
    }

    class Ripple : Entity
    {
        Red_Boss parent;

        public Ripple(Red_Boss parent) : base(parent.Position, "red_boss_ripple", 48,8,Drawing.DrawOrder.BG_ENTITIES)
        {
            AddAnimation("a", CreateAnimFrameArray(0, 1), 12);
            Play("a");
            this.parent = parent;
        }

        public override void Update()
        {
            base.Update();
            Position = parent.Position + new Vector2(-11, 17);
        }
    }
}
