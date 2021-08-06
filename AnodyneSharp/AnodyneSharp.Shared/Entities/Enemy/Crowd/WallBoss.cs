using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Crowd
{
    [NamedEntity, Enemy]
    public class WallBoss : Entity
    {
        Wall wall = new();
        Face face = new();

        EntityPool<DeathExplosion> explosions = new(8, () => new());

        IEnumerator state;

        Player player;

        EntityPreset preset;

        public WallBoss(EntityPreset preset, Player p) : base(preset.Position, Drawing.DrawOrder.ENTITIES)
        {
            visible = false;
            player = p;
            this.preset = preset;

            wall.opacity = face.opacity = 0;

            state = StateLogic();
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
        }

        IEnumerator StateLogic()
        {
            VolumeEvent volume = new(0f, 0.3f);
            GlobalState.SpawnEntity(volume);

            while (MapUtilities.GetInGridPosition(player.Position).Y > 80)
            {
                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("wallboss", "before_fight");

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            volume.exists = false;
            SoundManager.PlaySong("crowd_boss");

            while (!MathUtilities.MoveTo(ref face.opacity, 1, 0.18f))
            {
                wall.opacity = face.opacity; //TODO: add hands
                yield return null;
            }

            //Fight starts for real
            while (face.Health > 0)
            {
                yield return null;
            }

            //DIE
            //stop the hands
            SoundManager.StopSong();
            SoundManager.PlaySoundEffect("sun_guy_death_short");

            GlobalState.flash.Flash(3f, Color.White);

            while (GlobalState.flash.Active())
            {
                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("wallboss", "after_fight");
            SoundManager.PlaySoundEffect("talk_death");

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            float explosion_timer = 0f;
            while (!MathUtilities.MoveTo(ref face.opacity, 0f, 0.18f))
            {
                wall.opacity = face.opacity; //TODO: add hands opacity
                GlobalState.screenShake.Shake(0.02f, 0.1f);
                explosion_timer += GameTimes.DeltaTime;
                if (explosion_timer > 0.15f)
                {
                    explosion_timer = 0f;
                    explosions.Spawn((e)=>e.Spawn());
                    SoundManager.PlayPitchedSoundEffect("hit_wall", 0f, 0.3f);
                }
                yield return null;
            }

            float post_death_timer = 1f;
            while (post_death_timer > 0f)
            {
                post_death_timer -= GameTimes.DeltaTime;
                yield return null;
            }

            preset.Alive = exists = false;
            foreach (Entity e in SubEntities())
            {
                e.exists = false;
            }

            SoundManager.PlaySong("crowd");
            GlobalState.events.BossDefeated.Add("CROWD");

            yield break;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { wall, face }.Concat(explosions.Entities);
        }

        class DeathExplosion : Entity
        {
            public DeathExplosion() : base(Vector2.Zero, "enemy_explode_2", 24, 24, Drawing.DrawOrder.FG_SPRITES)
            {
                AddAnimation("explode", CreateAnimFrameArray(0, 1, 2, 3, 4), 14, false);
            }

            public override void Update()
            {
                base.Update();
                if (_curAnim.Finished)
                {
                    exists = false;
                }
            }

            public void Spawn()
            {
                Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
                Position = tl + new Vector2(GlobalState.RNG.Next(0, 160 - 24), GlobalState.RNG.Next(0, 32));
                Play("explode");
            }
        }
    }

    [Collision(typeof(Broom))]
    class Face : Entity
    {
        public int Health { get; private set; } = 12;

        float hit_timeout = 0f;
        bool invincible = true;

        IEnumerator state;

        EntityPool<Bullet> bullets;

        public Face() : base(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + Vector2.UnitX * (160 - 64) / 2, "f_wallboss_face", 64, 32, Drawing.DrawOrder.ENTITIES)
        {
            AddAnimation("normal", CreateAnimFrameArray(0, 2), 5);
            AddAnimation("hurt", CreateAnimFrameArray(3, 5), 14);
            AddAnimation("charge", CreateAnimFrameArray(1), 5);
            AddAnimation("shoot", CreateAnimFrameArray(4), 10);
            Play("normal");

            state = StateLogic();

            bullets = new(8, () => new(this));
        }

        public override void Update()
        {
            base.Update();
            MathUtilities.MoveTo(ref hit_timeout, 0, 1);
            if (opacity == 1f)
            {
                state.MoveNext();
            }
        }

        IEnumerator StateLogic()
        {
            while (true)
            {
                invincible = false;
                float charge_t = 0f;
                Play("normal");
                while (charge_t < 3f)
                {
                    charge_t += GameTimes.DeltaTime;
                    if (hit_timeout == 0f)
                    {
                        Play("normal");
                    }
                    yield return null;
                }

                invincible = true;
                Play("charge");

                while (charge_t < 4f)
                {
                    charge_t += GameTimes.DeltaTime;
                    yield return null;
                }

                Play("shoot");
                SoundManager.PlaySoundEffect("wb_shoot");

                bullets.Spawn((b) => b.Spawn(), 8);

                while (bullets.Alive != 0)
                {
                    yield return null;
                }
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return bullets.Entities;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (!invincible && hit_timeout <= 0)
            {
                --Health;
                if (Health == 0)
                {
                    SoundManager.PlaySoundEffect("wb_moan_2");
                    foreach (Entity e in bullets.Entities)
                    {
                        e.exists = false;
                    }
                    opacity = 0.99f; //disable logic
                }
                else
                {
                    SoundManager.PlaySoundEffect("wb_moan");
                }
                hit_timeout = 1;
                Play("hurt");
            }
        }

        [Collision(typeof(Player))]
        class Bullet : Entity
        {
            Face parent;

            Parabola_Thing parabola;

            public Bullet(Face parent) : base(parent.Center, "wallboss_bullet", 8, 8, Drawing.DrawOrder.FG_SPRITES)
            {
                AddAnimation("move", CreateAnimFrameArray(0, 1), 12);
                AddAnimation("explode", CreateAnimFrameArray(2, 3, 4), 10, false);

                shadow = new(this, Vector2.Zero);

                this.parent = parent;
            }

            public override void Update()
            {
                base.Update();
                if (CurAnimName == "move" && parabola.Tick())
                {
                    Play("explode");
                    velocity = Vector2.Zero;
                    SoundManager.PlaySoundEffect("4sht_pop");
                }
                else if (_curAnim.Finished)
                {
                    exists = false;
                }
            }

            public void Spawn()
            {
                Position = parent.Center;
                parabola = new(this, GlobalState.RNG.Next(16, 24), 1f + (float)GlobalState.RNG.NextDouble() / 2);
                Play("move");
                velocity = new(GlobalState.RNG.Next(-30, 30), 30);
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                if (other is Player p && CurAnimName == "move")
                {
                    p.ReceiveDamage(1);
                }
            }
        }
    }

    [Collision(PartOfMap = true)]
    class Wall : Entity
    {
        public Wall() : base(MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid), "wallboss_wall", 160, 32, Drawing.DrawOrder.VERY_BG_ENTITIES)
        {
            AddAnimation("move", CreateAnimFrameArray(0, 1), 4);
            Play("move");
            immovable = true;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (opacity != 0f)
            {
                Separate(this, other);
            }
        }
    }
}
