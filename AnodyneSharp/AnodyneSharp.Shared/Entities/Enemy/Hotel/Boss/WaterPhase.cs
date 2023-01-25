using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Entities.Interactive;
using AnodyneSharp.Entities.Interactive.Npc.Hotel;
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
    //No Enemy attribute to make north gate only open when entire boss has been defeated
    [NamedEntity("Eye_Boss",null,0), Collision(typeof(Player),typeof(Broom)), AlwaysSpawn]
    public class WaterPhase : Entity
    {
        EnemyMarker death_marker = new();

        int health = 6;

        bool broom_hit = false, player_hit = false;

        IEnumerator state;
        EntityPool<Bullet> bullets;

        CompositeSpriteRenderer renderer => sprite as CompositeSpriteRenderer;
        ISpriteRenderer eyelid => renderer.Renderers[2];

        Player player;

        public WaterPhase(EntityPreset preset, Player p) : base(preset.Position,EyebossPreview.GetSprite(),Drawing.DrawOrder.BG_ENTITIES)
        {
            player = p;
            if(GlobalState.events.BossDefeated.Contains(GlobalState.CURRENT_MAP_NAME))
            {
                //Full boss is defeated, open both gates and do nothing beyond that
                exists = false;
                death_marker.exists = false;
                GlobalState.PUZZLES_SOLVED++;
                return;
            }
            else if(SoundManager.CurrentSongName == "hotel-boss")
            {
                //Came back up from second phase screen
                (GlobalState.Map as MapData.Map).IgnoreMusicNextUpdate();
                GlobalState.PUZZLES_SOLVED++;
                preset.Alive = exists = false;
                return;
            }
            else
            {
                preset.Alive = true;
            }

            Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + Vector2.One * 16;

            p.grid_entrance = tl + new Vector2(80, 20);
            
            bullets = new(8, () => new Bullet(new Rectangle(tl.ToPoint(), new Point(16 * 8, 16 * 9))));

            state = State(tl, preset, p);
        }

        public static bool KeepInRect(Entity e, Rectangle r)
        {
            bool ret = false;
            Rectangle hitbox = e.Hitbox;
            if((hitbox.Left < r.Left && e.velocity.X < 0) || (hitbox.Right > r.Right && e.velocity.X > 0))
            {
                e.velocity.X *= -1;
                ret = true;
            }
            if((hitbox.Top < r.Top && e.velocity.Y < 0) || (hitbox.Bottom > r.Bottom && e.velocity.Y > 0))
            {
                e.velocity.Y *= -1;
                ret = true;
            }
            return ret;
        }

        IEnumerator State(Vector2 tl, EntityPreset preset, Player p)
        {
            VolumeEvent quiet = new(0, 0.007f * 60);
            GlobalState.SpawnEntity(quiet);

            while(p.Position.Y < tl.Y + 24)
            {
                yield return null;
            }
            
            if(!Dialogue.DialogueManager.IsSceneDirty("eyeboss","before_fight"))
            {
                GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("eyeboss", "before_fight");
                while (!GlobalState.LastDialogueFinished)
                    yield return null;
            }

            quiet.exists = false;
            SoundManager.PlaySong("hotel-boss");
            (GlobalState.Map as MapData.Map).IgnoreMusicNextUpdate();
            eyelid.PlayAnim("open");

            //Time for fight

            Point box_tl = (tl + new Vector2(12, 16)).ToPoint();
            Point box_br = (tl + new Vector2(16 * 7 + 4, 16 * 8)).ToPoint();

            IEnumerator fight = FightLogic(p);
            while(health > 0)
            {
                fight.MoveNext();
                if(KeepInRect(this,new Rectangle(box_tl,box_br-box_tl)))
                {
                    SoundManager.PlaySoundEffect("wb_tap_ground");
                }
                yield return null;
            }

            velocity = Vector2.UnitY * 40;
            GlobalState.SpawnEntity(new HealthPickup(Position, true) { exists = true });

            foreach(Bullet b in bullets.Entities)
            {
                b.Collided(null); //kill off bullets
            }

            while (Position.Y < tl.Y + 16 * 6)
                yield return null;

            while(!MathUtilities.MoveTo(ref opacity, 0, 3))
            {
                y_push += GameTimes.DeltaTime*60;
                yield return null;
            }

            GlobalState.PUZZLES_SOLVED++;
            exists = false;

            preset.Alive = false;
            EntityPreset land_phase = EntityManager.GetLinkGroup(preset.LinkID).Where(e => e != preset).First();
            land_phase.Activated = false; //Set land phase to spawn in its transition-to-land movement

            yield break;
        }

        IEnumerator FightLogic(Player p)
        {
            while(true)
            {
                IEnumerator current = SpeedUpOnHit(p);
                while(current.MoveNext())
                    yield return null;
                current = WaitingOnHit(p);
                while (current.MoveNext())
                    yield return null;
            }
        }

        IEnumerator WaitingOnHit(Player p)
        {
            float t = 0;
            float[] times = new float[] { 0, 0.65f, 0.7f, 0.8f, 0.9f, 0.95f, 1.1f };

            while(!broom_hit)
            {
                t += GameTimes.DeltaTime;
                if(t >= times[health])
                {
                    t = 0;
                    if(bullets.Spawn(b=>b.Spawn(Position)))
                    {
                        eyelid.PlayAnim("blink");
                    }
                }
                yield return null;
            }

            eyelid.PlayAnim("closed");
            velocity.X = MathF.Min(20, GlobalState.RNG.Next(0, 100));
            velocity.Y = MathF.Sqrt(100 * 100 - velocity.X * velocity.X);
            if (p.facing == Facing.UP) velocity.Y *= -1;
            if (p.facing == Facing.RIGHT) velocity.X *= -1;
            yield break;
        }

        IEnumerator SpeedUpOnHit(Player p)
        {
            float tm = 0;
            while(tm < 0.8)
            {
                tm += GameTimes.DeltaTime;

                if (player_hit) p.ReceiveDamage(1);
                if(broom_hit)
                {
                    tm = 0;
                    velocity *= 1.5f;
                }
                yield return null;
            }
            eyelid.PlayAnim("blink");
            velocity = Vector2.One * (6 - health) * 15;
            yield break;
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
            broom_hit = player_hit = false;
            Vector2 dir = (player.VisualCenter - VisualCenter);
            dir.Normalize();
            renderer.RenderProperties[1].Position = EyebossPreview.Eye_Center + dir * 2;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if(other is Broom && !_flickering)
            {
                broom_hit = true;
                health--;
                SoundManager.PlaySoundEffect("broom_hit");
                Flicker(2.3f);
            }
            else if(other is Player)
            {
                player_hit = true;
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            if(bullets is null)
            {
                return new List<Entity>() { death_marker };
            }
            return new List<Entity>() { death_marker }.Concat(bullets.Entities);
        }

        [Collision(typeof(Player),typeof(Broom))]
        class Bullet : Entity
        {
            Rectangle area;

            public static AnimatedSpriteRenderer GetSprite() => new("eye_boss_bullet", 16, 16,
                new Anim("move", new int[] { 0, 1 }, 12),
                new Anim("pop", new int[] { 2, 3, 4, 5 }, 12, false)
                );

            public Bullet(Rectangle area) : base(Vector2.Zero,GetSprite(),Drawing.DrawOrder.FG_SPRITES)
            {
                this.area = area;

                width = height = 8;
                CenterOffset();
            }

            public void Spawn(Vector2 position)
            {
                Position = position;
                velocity = Vector2.One * 50;
                Play("move");
                SoundManager.PlaySoundEffect("gasguy_shoot");
            }

            public override void Update()
            {
                base.Update();
                if (AnimFinished) exists = false;
                KeepInRect(this, area);
            }

            public override void Collided(Entity other)
            {
                base.Collided(other);
                if(velocity == Vector2.Zero)
                {
                    return;
                }
                velocity = Vector2.Zero;
                Play("pop");
                SoundManager.PlaySoundEffect("dustpoof");
                if(other is Player p)
                {
                    p.ReceiveDamage(1);
                }
            }
        }
    }
}
