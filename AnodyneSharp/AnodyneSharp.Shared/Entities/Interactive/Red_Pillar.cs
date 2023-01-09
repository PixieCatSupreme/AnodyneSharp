using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities
{
    [NamedEntity, Collision(PartOfMap = true)]
    public class Red_Pillar : Entity
    {
        Chain collider;
        EntityPreset _preset;

        public static AnimatedSpriteRenderer GetSprite() => new("red_pillar", 16, 80,
            new Anim("idle", new int[] { 0 },1),
            new Anim("move", new int[] { 1, 2 },8)
            );

        public Red_Pillar(EntityPreset preset, Player p) : base(preset.Position, GetSprite(), Drawing.DrawOrder.BG_ENTITIES)
        {
            immovable = true;
            _preset = preset;
            collider = new(this);
        }

        public override void Collided(Entity other)
        {
            Separate(this, other);
        }

        public override IEnumerable<Entity> SubEntities()
        {
            return new List<Entity>() { collider };
        }

        IEnumerator<CutsceneEvent> Cutscene()
        {
            Play("move");
            Entity ripple = new(Position + Vector2.UnitY * (height - 15), new AnimatedSpriteRenderer("red_pillar_ripple", 16, 16, new Anim("move",new int[] { 0, 1 },8)), Drawing.DrawOrder.ENTITIES);
            GlobalState.SpawnEntity(ripple);

            Flicker(1.5f);
            Sounds.SoundManager.PlaySoundEffect("broom_hit");

            while (!MathUtilities.MoveTo(ref y_push, height, height / 1.5f) || _flickering)
            {
                yield return null;
            }

            int required = 1;
            char c = 'n';

            switch(_preset.Frame)
            {
                case 1:
                case 2:
                    required = 2;
                    break;
                case 3:
                    c = 'l';
                    break;
                case 4:
                    c = 'r';
                    break;
            }

            string eventName = $"red_cave_{c}_ss";
            GlobalState.events.IncEvent(eventName);
            if(GlobalState.events.GetEvent(eventName) == required)
            {
                Point warp = c switch
                {
                    'n' => new(3, 3),
                    'l' => new(2, 4),
                    _ => new(4, 4),
                };
                Entity door = new(warp.ToVector2() * 160 + new Vector2(48,48), "red_cave_left", 64, 64, Drawing.DrawOrder.ENTITIES);
                door.y_push = door.height;

                yield return new EntityEvent(Enumerable.Repeat(door, 1));
                yield return new WarpEvent("REDSEA", warp);

                Sounds.SoundManager.PlaySoundEffect("red_cave_rise");

                while(door.y_push > 0)
                {
                    float time = 0.02f;
                    GlobalState.screenShake.Shake(0.05f, 0.5f);
                    while(time > 0)
                    {
                        time -= GameTimes.DeltaTime;
                        yield return null;
                    }
                    door.y_push--;
                }

                float timer = 1.5f;
                while(timer > 0)
                {
                    timer -= GameTimes.DeltaTime;
                    yield return null;
                }
            }

            _preset.Alive = false;
            exists = false;
            ripple.exists = false;
            yield break;
        }

        [Collision(typeof(Broom))]
        class Chain : Entity
        {
            Red_Pillar _parent;

            public Chain(Red_Pillar parent) : base(parent.Position + Vector2.UnitY * 16, 16, 16)
            {
                _parent = parent;
            }

            public override void Collided(Entity other)
            {
                GlobalState.StartCutscene = _parent.Cutscene();
                exists = false;
            }
        }
    }
}
