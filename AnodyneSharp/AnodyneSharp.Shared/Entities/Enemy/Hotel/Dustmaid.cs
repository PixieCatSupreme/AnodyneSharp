using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Hotel
{
    [NamedEntity(), Enemy, Collision(typeof(Player), typeof(Broom), KeepOnScreen = true, MapCollision = true)]
    class Dustmaid : HealthDropper
    {
        private Player _player;
        private IEnumerator _stateLogic;

        private int _health;
        private bool _chasing;

        public static AnimatedSpriteRenderer GetSprite() => new("dustmaid", 16, 24,
            new Anim("idle", new int[] { 0 },1),
            new Anim("turn_dark", new int[] { 1, 2, 1, 2 },12,false),
            new Anim("move_r", new int[] { 5, 6 },5),
            new Anim("move_l", new int[] { 5, 6 }, 5),
            new Anim("move_d", new int[] { 3, 4 }, 5),
            new Anim("move_u", new int[] { 7, 8 }, 5)
            );

        public Dustmaid(EntityPreset preset, Player player)
            : base(preset, preset.Position, GetSprite(), DrawOrder.ENTITIES, healthDropChance: 0.7f)
        {
            _player = player;

            opacity = 0.7f;

            width = 8;
            height = 18;

            CenterOffset();

            offset += new Vector2(1, 2);

            _health = 3;

            _stateLogic = StateLogic();
        }

        public override void Update()
        {
            base.Update();

            _stateLogic.MoveNext();
        }

        private IEnumerator StateLogic()
        {
            while (_player.broom.dust == null)
            {
                yield return null;
            }

            Play("turn_dark");
            SoundManager.PlaySoundEffect("dustmaid_alert");
            opacity = 1;

            _chasing = true;

            while (!AnimFinished)
            {
                yield return null;
            }

            while (exists)
            {
                FaceTowards(_player.Position);

                PlayFacing("move");

                MoveTowards(_player.Position, 20);

                yield return null;
            }

            yield break;
        }

        public override void Collided(Entity other)
        {
            base.Collided(other);

            if (other is Player p)
            {
                p.ReceiveDamage(1);
            }
            else if (other is Broom && _chasing && !_flickering)
            {
                _health--;

                Flicker(1);

                SoundManager.PlaySoundEffect("broom_hit");

                if (_health <= 0)
                {
                    Die();

                    GlobalState.SpawnEntity(new Explosion(this));
                }
            }
        }

        protected override void AnimationChanged(string name)
        {
            base.AnimationChanged(name);

            if (name.EndsWith('l'))
            {
                _flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                _flip = SpriteEffects.None;
            }
        }
    }
}
