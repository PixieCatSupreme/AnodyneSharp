using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using System.Collections;

namespace AnodyneSharp.Entities.Enemy.Go
{
    [Collision(typeof(Broom))]
    abstract class BigThorn : Entity
    {
        public int Health = 3;

        public IEnumerator state;

        public static AnimatedSpriteRenderer GetSprite(string tex, int offset, ILayerType layer)
        {
            return new(tex, 64, 80, layer,
                new Anim("off", new int[] { offset, offset + 1, offset + 2, (offset + 3) % 4 }, 4),
                new Anim("hit", new int[] { 4, 6, 4, 6, 4, 6, 4, 6, 4, 6, 4, 6 }, 15, false),
                new Anim("hitloop", new int[] { 4, 6 }, 15),
                new Anim("hurt", new int[] { 7, 8 }, 4),
                new Anim("active", new int[] { 4, 5 }, 5)
                );
        }

        public BigThorn(Vector2 pos, string tex, int offset, ILayerType layer) : base(pos, GetSprite(tex,offset,layer))
        {
            immovable = true;

            height = 63;
            width = 24;
        }

        public override void Update()
        {
            base.Update();
            if(!state?.MoveNext() ?? false)
            {
                state = null;
            }
            if(AnimFinished)
            {
                Play("off");
            }
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);
            if(other is Broom && CurAnimName == "hurt")
            {
                Play("hit");
                Health--;
                GlobalState.screenShake.Shake(0.03f, 0.4f);
                GlobalState.flash.Flash(0.6f, new Color(255, 17, 17));
                SoundManager.PlaySoundEffect("wb_hit_ground");
            }
        }

        public abstract IEnumerator GetAttacked(BigThorn blue, Player player);
    }
}
