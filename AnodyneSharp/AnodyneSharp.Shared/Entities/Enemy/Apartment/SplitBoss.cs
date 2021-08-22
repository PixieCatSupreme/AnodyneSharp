using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Events;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Apartment
{
    [Collision(typeof(Player))]
    class BaseSplitBoss : Entity
    {
        public bool damage_player = false;

        public BaseSplitBoss(Vector2 position) : base(position, "splitboss", 24, 32, Drawing.DrawOrder.ENTITIES)
        {
            width = 16;
            height = 20;
            CenterOffset();

            Solid = false;
            immovable = true;

            AddAnimation("float", CreateAnimFrameArray(0, 1, 2, 1), 5); //facing downwards, idle
            AddAnimation("idle_r", CreateAnimFrameArray(4, 5, 6, 5), 5);
            AddAnimation("dash_r", CreateAnimFrameArray(10, 11), 8);
            AddAnimation("dash_d", CreateAnimFrameArray(8, 9), 8);
            AddAnimation("die", CreateAnimFrameArray(0, 3), 14);
            Play("float");
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (other is Player p && damage_player)
            {
                p.ReceiveDamage(1);
            }
        }

        protected override void AnimationChanged(string name)
        {
            base.AnimationChanged(name);
            if (name.EndsWith("_r"))
            {
                offset.Y = 6;
            }
            else
            {
                CenterOffset(false);
            }
        }
    }

    [NamedEntity("Splitboss"), Collision(typeof(Broom))]
    class SplitBoss : BaseSplitBoss
    {
        Player player;
        EntityPreset _preset;

        Copy[] copies = new Copy[2]
        {
            new(true),
            new(false)
        };

        IEnumerator state;

        //top left of main fight area
        Vector2 tl = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) + new Vector2(32, 48);

        bool hit_this_frame = false;
        bool vulnerable = false;
        float invuln_time = 0f;

        int health = 13;

        int Phase => health switch
        {
            > 8 => 0,
            > 4 => 1,
            _ => 2
        };

        public SplitBoss(EntityPreset preset, Player p) : base(preset.Position)
        {
            _preset = preset;
            player = p;

            Position = tl + new Vector2(48 - width / 2, 32);

            p.grid_entrance = tl + new Vector2(42, 40);

            state = Intro();
        }

        public override void Update()
        {
            base.Update();
            state.MoveNext();
            MathUtilities.MoveTo(ref invuln_time, 0, 1);
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            hit_this_frame = false;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (other is Broom)
            {
                hit_this_frame = true;
                if (vulnerable && invuln_time == 0)
                {
                    invuln_time = 0.8f;
                    Flicker(invuln_time);
                    SoundManager.PlaySoundEffect("sb_hurt");
                    SoundManager.PlaySoundEffect("broom_hit");
                    health--;
                    if (health == 0)
                    {
                        vulnerable = false;
                        state = Die();
                    }
                }
            }
        }

        IEnumerator Intro()
        {
            VolumeEvent volume = new(0, 0.3f);

            GlobalState.SpawnEntity(volume);

            while (!hit_this_frame)
            {
                yield return null;
            }

            GlobalState.Dialogue = DialogueManager.GetDialogue("splitboss", "before_fight");

            while (!GlobalState.LastDialogueFinished)
            {
                yield return null;
            }

            volume.exists = false;

            SoundManager.PlaySoundEffect("sb_split");
            SoundManager.PlaySong("apartment-boss");

            visible = false;
            foreach (Copy c in copies)
            {
                c.exists = true;
                c.Position = Position;
                c.velocity.X = 50;
            }
            copies[0].velocity.X *= -1;

            while (!MathUtilities.MoveTo(ref copies[0].opacity, 0, 0.6f))
            {
                copies[1].opacity = copies[0].opacity;
                yield return null;
            }

            opacity = 0;
            visible = true;
            Position.Y = tl.Y - 32;
            copies[0].Position.X = Position.X - 40;
            copies[1].Position.X = Position.X + 40;
            foreach (Copy c in copies)
            {
                c.velocity = Vector2.Zero;
                c.Position.Y = Position.Y;
                c.Play("float", true); //sync up float animation
            }
            Play("float", true);

            while (!MathUtilities.MoveTo(ref opacity, 1, 0.6f))
            {
                copies[0].opacity = copies[1].opacity = opacity;
                yield return null;
            }

            while (!MathUtilities.MoveTo(ref copies[0].Position.X, Position.X, 30) | !MathUtilities.MoveTo(ref copies[1].Position.X, Position.X, 30))
            {
                Flicker(0.2f);
                copies[0].Flicker(0.2f); copies[1].Flicker(0.2f);
                yield return null;
            }

            copies[0].exists = copies[1].exists = false;

            state = Split(); //TODO: change to actual first attack

            yield break;
        }

        IEnumerator Die()
        {

            _preset.Alive = exists = false;
            GlobalState.events.BossDefeated.Add(GlobalState.CURRENT_MAP_NAME);
            yield break;
        }

        IEnumerator Split()
        {
            visible = true;
            vulnerable = false;
            damage_player = false;

            Play("float");

            Position = tl + new Vector2(40, -16);

            Flicker(1.5f);

            while (!hit_this_frame)
            {
                yield return null;
            }

            opacity = copies[0].opacity = copies[1].opacity = 1f;
            copies[0].exists = copies[1].exists = true;
            copies[0].Play("idle_r"); copies[1].Play("idle_r");
            copies[0].Position = copies[1].Position = Position;

            velocity.Y = -40;
            copies[0].velocity.X = -40;
            copies[1].velocity.X = 40;

            while (!MathUtilities.MoveTo(ref opacity, 0, 1.2f))
            {
                copies[0].opacity = copies[1].opacity = opacity;
                yield return null;
            }

            velocity = copies[0].velocity = copies[1].velocity = Vector2.Zero;
            opacity = copies[0].opacity = copies[1].opacity = 0.7f;

            float tm = 0f;

            while (tm < 1f)
            {
                tm += GameTimes.DeltaTime;
                Position = player.Position - Vector2.UnitY * 24;
                copies[0].Position = player.Position + Vector2.UnitX * 25;
                copies[1].Position = player.Position - Vector2.UnitX * 25;
                yield return null;
            }

            //Warning for copies
            copies[0].Flicker(0.7f);
            copies[1].Flicker(0.7f);
            while (copies[0]._flickering)
            {
                Position = player.Position - Vector2.UnitY * 24;
                yield return null;
            }

            copies[0].damage_player = copies[1].damage_player = true;

            float split_vel = 40 + 10 * Phase;

            copies[0].velocity.X = -split_vel;
            copies[1].velocity.X = split_vel;
            SoundManager.PlaySoundEffect("sb_dash");

            while (!MathUtilities.MoveTo(ref copies[0].opacity, 0, 1.2f) | !MathUtilities.MoveTo(ref copies[1].opacity, 0, 1.2f))
            {
                Position = player.Position - Vector2.UnitY * 24;
                yield return null;
            }

            copies[0].exists = copies[1].exists = copies[0].damage_player = copies[1].damage_player = false;

            Flicker(1);

            while (_flickering)
            {
                yield return null;
            }

            vulnerable = true;
            damage_player = true;

            velocity.Y = split_vel;

            while (!MathUtilities.MoveTo(ref opacity, 0, 1.2f))
            {
                yield return null;
            }

            opacity = 1f;
            vulnerable = false;
            damage_player = false;
            velocity = Vector2.Zero;

            //TODO: decide next attack
            state = Split();

            yield break;
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return copies;
        }
    }

    class Copy : BaseSplitBoss
    {
        public Copy(bool right) : base(Vector2.Zero)
        {
            if (right)
            {
                _flip = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
            }
            exists = false;
        }
    }
}
