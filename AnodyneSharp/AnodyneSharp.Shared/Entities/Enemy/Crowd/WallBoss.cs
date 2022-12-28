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
        LHand lhand = new();
        RHand rhand = new();
        Laser laser = new();

        EntityPool<DeathExplosion> explosions = new(8, () => new());

        IEnumerator state;

        Player player;

        EntityPreset preset;

        int Phase => face.Health switch
        {
            > 10 => 0,
            > 5 => 1,
            _ => 2
        };

        public WallBoss(EntityPreset preset, Player p) : base(preset.Position, Drawing.DrawOrder.ENTITIES)
        {
            visible = false;
            player = p;
            this.preset = preset;

            wall.opacity = rhand.opacity = lhand.opacity = face.opacity = 0;

            state = StateLogic();

            p.grid_entrance = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(86, 81);
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
                wall.opacity = rhand.opacity = lhand.opacity = face.opacity;
                yield return null;
            }

            //Fight starts for real
            IEnumerator fight_logic = FightLogic();
            while (face.Health > 0)
            {
                fight_logic.MoveNext();
                yield return null;
            }

            //DIE
            lhand.state = rhand.state = null;
            laser.exists = false;
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
                wall.opacity = rhand.opacity = lhand.opacity = face.opacity;
                GlobalState.screenShake.Shake(0.02f, 0.1f);
                explosion_timer += GameTimes.DeltaTime;
                if (explosion_timer > 0.15f)
                {
                    explosion_timer = 0f;
                    explosions.Spawn((e) => e.Spawn());
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

        IEnumerator FightLogic()
        {
            while (true)
            {
                lhand.state = lhand.Float();
                rhand.state = rhand.Float();
                float t = 2f;
                while (t > 0f)
                {
                    t -= GameTimes.DeltaTime;
                    yield return null;
                }
                double r = GlobalState.RNG.NextDouble();
                IEnumerator attack;
                switch (Phase)
                {
                    case 0:
                        if (r <= 0.7)
                        {
                            attack = StompAttack();
                        }
                        else
                        {
                            attack = PushAttack();
                        }
                        break;
                    case 1:
                        if (r <= 0.35)
                        {
                            attack = LaserAttack();
                        }
                        else if (r <= 0.7)
                        {
                            attack = StompAttack();
                        }
                        else
                        {
                            attack = PushAttack();
                        }
                        break;
                    case 2:
                    default:
                        if (r <= 0.5)
                        {
                            attack = LaserAttack();
                        }
                        else if (r <= 0.75)
                        {
                            attack = StompAttack();
                        }
                        else
                        {
                            attack = PushAttack();
                        }
                        break;
                }
                while (attack.MoveNext())
                {
                    yield return null;
                }
            }
        }

        IEnumerator StompAttack()
        {
            lhand.Play("stomp");
            rhand.Play("stomp");
            lhand.state = null;
            rhand.state = rhand.GoTo(rhand.init_pt, 30);
            while(rhand.state != null)
            {
                yield return null;
            }
            rhand.state = rhand.Stomp();
            lhand.state = lhand.Stomp(Phase, player);
            while(lhand.state != null)
            {
                yield return null;
            }
            rhand.state = rhand.Reset(180);
            lhand.state = lhand.Reset(180);
            while(rhand.state != null || lhand.state != null)
            {
                yield return null;
            }
            yield break;
        }

        IEnumerator LaserAttack()
        {
            lhand.state = lhand.GoTo(lhand.init_pt, 180);
            rhand.state = rhand.GoTo(rhand.init_pt, 180);
            lhand.Play("shoot");
            rhand.Play("shoot");
            while (lhand.state != null || rhand.state != null)
            {
                yield return null;
            }

            laser.exists = true;
            laser.state = laser.Attack();
            lhand.state = lhand.Follow(laser);
            rhand.state = rhand.Follow(laser);

            while (laser.state != null)
            {
                yield return null;
            }

            lhand.state = lhand.Reset(90);
            rhand.state = rhand.Reset(90);
            while (lhand.state != null || rhand.state != null)
            {
                yield return null;
            }
            yield break;
        }

        IEnumerator PushAttack()
        {
            int num_pushes = Phase == 0 ? 2 : 3;

            while (num_pushes-- > 0)
            {
                lhand.state = rhand.state = null;
                double double_push_chance = Phase switch
                {
                    0 => 0.5,
                    1 => 0.7,
                    _ => 0.8
                };
                int type; //0: right hand, 1: left hand, 2: both
                if (GlobalState.RNG.NextDouble() < double_push_chance)
                {
                    type = 2;
                }
                else if (GlobalState.RNG.NextDouble() < 0.5)
                {
                    type = 1;
                }
                else
                {
                    type = 0;
                }

                if (type != 0)
                {
                    lhand.state = lhand.GoTo(lhand.init_pt - Vector2.UnitY * 16, 30);
                }
                if (type != 1)
                {
                    rhand.state = rhand.GoTo(rhand.init_pt - Vector2.UnitY * 16, 30);
                }

                while ((rhand.state != null) || (lhand.state != null))
                {
                    yield return null;
                }

                SoundManager.PlaySoundEffect("slasher_atk");

                if (type != 0)
                {
                    lhand.state = lhand.Push(Phase);
                }
                if (type != 1)
                {
                    rhand.state = rhand.Push(Phase);
                }

                while ((rhand.state != null) || (lhand.state != null))
                {
                    yield return null;
                }

                lhand.state = lhand.Reset(180);
                rhand.state = rhand.Reset(180);

                while ((rhand.state != null) || (lhand.state != null))
                {
                    yield return null;
                }
            }
            yield break;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { wall, face, lhand, rhand, laser }.Concat(explosions.Entities);
        }

    }

    class DeathExplosion : Entity
    {
        public DeathExplosion() : base(Vector2.Zero, "enemy_explode_2", 24, 24, Drawing.DrawOrder.FG_SPRITES)
        {
            AddAnimation("explode", CreateAnimFrameArray(0, 1, 2, 3, 4), 14, false);
            exists = false;
        }

        public override void Update()
        {
            base.Update();
            if (AnimFinished)
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

    [Collision(typeof(Player))]
    class Laser : Entity
    {
        Vector2 start_loc;

        public IEnumerator state;

        public Laser() : base(Vector2.Zero, "wallboss_laser", 64, 10, Drawing.DrawOrder.BG_ENTITIES)
        {
            AddAnimation("charge", CreateAnimFrameArray(0));
            AddAnimation("attack", CreateAnimFrameArray(1, 2), 12);
            exists = false;
            visible = false;
            start_loc = Position = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(48, 32 + 11);
        }

        public override void Update()
        {
            base.Update();
            if (!state?.MoveNext() ?? false)
            {
                state = null;
                exists = false;
                visible = false;
            }
        }

        public IEnumerator Attack()
        {
            Play("charge");
            Flicker(1.3f);
            SoundManager.PlaySoundEffect("sun_guy_charge");
            while (_flickering)
            {
                yield return null;
            }

            Play("attack");

            velocity.Y = 50;
            while (MapUtilities.GetInGridPosition(Position).Y < 16 * 6 + 11)
            {
                yield return null;
            }

            velocity.Y = -65;
            while (MapUtilities.GetInGridPosition(Position).Y > 45)
            {
                yield return null;
            }

            velocity.Y = 80;
            while (MapUtilities.GetInGridPosition(Position).Y < 16 * 6 + 11)
            {
                yield return null;
            }

            velocity = Vector2.Zero;
            while (!MathUtilities.MoveTo(ref Position.Y, start_loc.Y, 110))
            {
                yield return null;
            }

            yield break;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (!_flickering && other is Player p && p.state != PlayerState.AIR)
            {
                p.ReceiveDamage(1);
            }
        }
    }

    [Collision(typeof(Player))]
    class Hand : Entity
    {
        protected Parabola_Thing stomp_parabola;
        bool is_right_hand;
        public Vector2 init_pt { get; private set; }

        DeathExplosion explosion = new();

        public IEnumerator state;

        public Hand(bool right) : base(Vector2.Zero, "f_wallboss_l_hand", 32, 32, Drawing.DrawOrder.ENTITIES)
        {
            Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid);
            if (right)
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                Position = tl + new Vector2(7 * 16, 32);
            }
            else
            {
                Position = tl + new Vector2(16, 32);
            }

            AddAnimation("idle", CreateAnimFrameArray(0));
            AddAnimation("stomp", CreateAnimFrameArray(1));
            AddAnimation("shoot", CreateAnimFrameArray(2));
            AddAnimation("push", CreateAnimFrameArray(3));
            Play("idle");

            stomp_parabola = new(this, right ? 32 : 64, right ? 1 : 2);
            is_right_hand = right;
            init_pt = Position;
            shadow = new(this, new(8,-12), ShadowType.Big);
            shadow.visible = false;
            shadow.HasVisibleHitbox = true;

            immovable = true;
        }

        public IEnumerator Float()
        {
            float t = 0f;
            while (true)
            {
                t += GameTimes.DeltaTime;
                Position.Y = init_pt.Y + 4 + (is_right_hand ? -1 : 1) * MathF.Sin(t * MathF.Tau) * 8;
                yield return null;
            }
        }

        public IEnumerator Push(int phase)
        {
            Play("push");
            velocity = new(GlobalState.RNG.Next(20, 60), 60 + phase * 30);
            if (is_right_hand)
            {
                velocity.X *= -1;
            }

            while (MapUtilities.GetInGridPosition(Position).Y < 6 * 16)
            {
                yield return null;
            }
            velocity = Vector2.Zero;

            yield break;
        }

        public IEnumerator Follow(Laser l)
        {
            while (true)
            {
                Position.Y = l.Position.Y - 11;
                yield return null;
            }
        }

        public IEnumerator GoTo(Vector2 pos, float speed)
        {
            while (!(MathUtilities.MoveTo(ref Position.X, pos.X, speed) & MathUtilities.MoveTo(ref Position.Y, pos.Y, speed)))
            {
                yield return null;
            }
            yield break;
        }

        public IEnumerator Reset(float speed)
        {
            velocity = Vector2.Zero;
            shadow.visible = false;
            IEnumerator reset = GoTo(init_pt, speed);
            while (reset.MoveNext() | !MathUtilities.MoveTo(ref offset.Y,0,120))
            {
                yield return null;
            }
            Play("idle");
            yield break;
        }

        protected void SpawnExplosion()
        {
            explosion.exists = true;
            explosion.Position = Position;
            explosion.Play("explode",true);
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (!shadow.visible)
            {
                Separate(this, other);
            }
            else if (shadow.Hitbox.Intersects(other.Hitbox) && offset.Y < 8 && other is Player p)
            {
                p.ReceiveDamage(1);
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(explosion, 1);
        }

        public override void Update()
        {
            base.Update();
            if (!state?.MoveNext() ?? false)
            {
                state = null;
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            shadow.SetFrame(offset.Y switch
            {
                >= 29 => 0,
                >= 24 => 1,
                >= 16 => 2,
                >= 8 => 3,
                _ => 4
            });
        }
    }

    class LHand : Hand
    {
        enum GroundPhase
        {
            None,
            Cracked,
            Full
        }
        GroundPhase ground_state = GroundPhase.None;

        public LHand() : base(false) { }

        public IEnumerator Stomp(int phase, Player p)
        {
            shadow.visible = true;
            int nr_stomps = phase + 2;
            while (nr_stomps-- > 0)
            {
                stomp_parabola.ResetTime();
                while(stomp_parabola.Progress() < 0.5f)
                {
                    MathUtilities.MoveTo(ref Position.X, p.Position.X, 60);
                    MathUtilities.MoveTo(ref Position.Y, p.Position.Y - 16, 60);
                    stomp_parabola.Tick();
                    yield return null;
                }
                SoundManager.PlaySoundEffect("fall_1");
                float wait_time = 0f;
                while(wait_time < 0.7f)
                {
                    wait_time += GameTimes.DeltaTime;
                    yield return null;
                }
                while(!stomp_parabola.Tick())
                {
                    yield return null;
                }
                GlobalState.screenShake.Shake(0.01f, 0.2f);
                SpawnExplosion();
                SoundManager.PlaySoundEffect("wb_hit_ground");

                if(phase != 0 && ground_state != GroundPhase.Full)
                {
                    GlobalState.screenShake.Shake(0.02f, 0.5f);
                    SoundManager.PlaySoundEffect("floor_crack");

                    int next_tile = ground_state == GroundPhase.None ? 71 : 81;
                    Point tl = GlobalState.TopLeftTile;
                    for(int x = 2; x < 8; ++x)
                    {
                        GlobalState.Map.ChangeTile(MapData.Layer.BG, new(tl.X + x, tl.Y + 6), next_tile);
                    }
                    ground_state = (ground_state == GroundPhase.None) ? GroundPhase.Cracked : GroundPhase.Full;
                }
                yield return null;
            }
            yield break;
        }
    }

    class RHand : Hand
    {
        public RHand() : base(true) { }

        public IEnumerator Stomp()
        {
            stomp_parabola.ResetTime();
            velocity.X = -30;
            shadow.visible = true;
            while (true)
            {
                if ((velocity.X < 0 && MapUtilities.GetInGridPosition(Position).X < 16)
                    || (velocity.X > 0 && MapUtilities.GetInGridPosition(Position).X > 16 * 9 - width))
                {
                    velocity *= -1;
                }
                if(stomp_parabola.Tick())
                {
                    SoundManager.PlaySoundEffect("wb_tap_ground");
                    SpawnExplosion();
                    stomp_parabola.ResetTime();
                }
                yield return null;
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
                else if (AnimFinished)
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
