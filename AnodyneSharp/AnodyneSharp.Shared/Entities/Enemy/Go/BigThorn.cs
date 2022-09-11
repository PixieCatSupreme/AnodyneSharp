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

        public BigThorn(Vector2 pos, string tex) : base(pos, tex, 64, 80, Drawing.DrawOrder.ENTITIES)
        {
            immovable = true;
            AddAnimation("hit", CreateAnimFrameArray(4, 6, 4, 6, 4, 6, 4, 6, 4, 6, 4, 6), 15, false);
            AddAnimation("hurt", CreateAnimFrameArray(7, 8), 4);
            AddAnimation("active", CreateAnimFrameArray(4, 5), 5);

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
            if(_curAnim.Finished)
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
