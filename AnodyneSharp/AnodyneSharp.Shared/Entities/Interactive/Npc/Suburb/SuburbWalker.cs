using AnodyneSharp.Dialogue;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Interactive.Npc.Suburb
{
    [NamedEntity("Suburb_Walker"), AlwaysSpawn, Collision(typeof(Player), typeof(Broom), MapCollision = true)]
    class SuburbWalker : Entity, Interactable
    {
        EntityPreset _preset;

        Entity blood;

        float walk_t_max;
        float walk_t;

        public static AnimatedSpriteRenderer GetSprite(int off) => new("suburb_walkers", 16, 16,
            new Anim("walk_d", new int[] { 0 + off, 1 + off }, 4),
            new Anim("walk_r", new int[] { 2 + off, 3 + off }, 4),
            new Anim("walk_u", new int[] { 4 + off, 5 + off }, 4),
            new Anim("walk_l", new int[] { 6 + off, 7 + off }, 4),
            new Anim("die", new int[] { 0 + off, 2 + off, 4 + off, 6 + off, 0 + off, 2 + off, 4 + off, 6 + off, 8 + off }, 8, false),
            new Anim("dead", new int[] { 8 + off }, 1)
            );

        public SuburbWalker(EntityPreset preset, Player p) : base(preset.Position, GetSprite(preset.Frame*9), Drawing.DrawOrder.ENTITIES)
        {
            _preset = preset;

            velocity = Vector2.UnitY * 20;
            
            walk_t_max = 1f + (float)GlobalState.RNG.NextDouble();
            walk_t = walk_t_max;

            blood = new(Vector2.Zero, new AnimatedSpriteRenderer("suburb_walkers", 16, 16, new Anim("a", new int[] { 63, 64, 65, 66 }, 3, false)), Drawing.DrawOrder.BG_ENTITIES)
            {
                exists = false
            };

            if (_preset.Alive == false)
            {
                velocity = Vector2.Zero;
                Play("dead");
                SpawnBlood();
            }
        }

        public override void Update()
        {
            base.Update();
            if (_preset.Alive)
            {
                if (touching != Touching.NONE)
                {
                    walk_t = 0;
                }

                Vector2 grid = MapUtilities.GetInGridPosition(Position);
                if ((grid.X < 5 && velocity.X < 0) || (grid.X + width > 155 && velocity.X > 0)
                  || (grid.Y < 5 && velocity.Y < 0) || (grid.Y + height > 155 && velocity.Y > 0))
                {
                    walk_t = 0;
                }

                walk_t -= GameTimes.DeltaTime;
                if (walk_t <= 0)
                {
                    (velocity.X, velocity.Y) = (velocity.Y, -velocity.X);
                    FaceTowards(Position + velocity);
                    PlayFacing("walk");
                    walk_t = walk_t_max;
                }
            }
            else if(AnimFinished && !blood.exists)
            {
                SpawnBlood();
            }
        }

        void SpawnBlood()
        {
            blood.exists = true;
            blood.Position = Position + new Vector2(GlobalState.RNG.Next(-3, 4), GlobalState.RNG.Next(-3, 4));
        }

        public bool PlayerInteraction(Facing player_direction)
        {
            if(blood.Frame == 66)
            {
                string scene = "words_" + _preset.Frame switch
                {
                    < 2 => "adult",
                    < 4 => "teen",
                    _ => "kid"
                };
                GlobalState.Dialogue = DialogueManager.RandomDialogue("suburb_walker", scene);
                return true;
            }
            return false;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if (other is Player p)
            {
                immovable = true;
                Touching old = touching;
                Separate(this, p);
                touching = old;
                immovable = false;
            }
            else if(other is Broom && _preset.Alive)
            {
                _preset.Alive = false;
                velocity = Vector2.Zero;
                Play("die");
                SoundManager.PlaySoundEffect("broom_hit");
                SoundManager.PlaySoundEffect("fall_in_hole");
                GlobalState.events.IncEvent("SuburbKilled");
            }
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return Enumerable.Repeat(blood, 1);
        }
    }
}
