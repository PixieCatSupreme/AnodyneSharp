using AnodyneSharp.Entities.Lights;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Bedroom
{
    [NamedEntity("Sun_Guy"), Enemy, Collision(typeof(Player), typeof(Broom))]
    public class Seer : Entity
    {
        Player _player;
        EntityPreset _preset;

        int _health = 7;

        SeerOrb[] _orbs;

        Dust[] dusts;

        SeerWave wave = new();

        IEnumerator _state;

        public Seer(EntityPreset preset, Player p) : base(preset.Position, "sun_guy", 16, 24, Drawing.DrawOrder.ENTITIES)
        {
            _player = p;
            _preset = preset;
            AddAnimation("float", CreateAnimFrameArray(0, 1, 2, 3, 4), 3);
            Play("float");
            _orbs = new SeerOrb[]
            {
                new(0,16,3f,this),
                new(2,32,2.4f,this)
            };

            dusts = new Dust[]
            {
                new(),new(),new()
            };

            _state = StateLogic();
            opacity = 0f;

        }

        public override void Update()
        {
            base.Update();
            _state.MoveNext();
        }

        IEnumerator StateLogic()
        {
            GlobalState.darkness.TargetAlpha(0.9f, 0.36f);
            SoundManager.StopSong();

            while (_player.Position.Y > Position.Y + 48)
            {
                MathUtilities.MoveTo(ref GlobalState.PlayerLight.scale, 1.7f, 1.2f);
                yield return null;
            }

            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("sun_guy", "before_fight");

            while (!GlobalState.LastDialogueFinished)
            {
                MathUtilities.MoveTo(ref GlobalState.PlayerLight.scale, 1.7f, 1.2f);
                MathUtilities.MoveTo(ref opacity, 1.0f, 0.42f);
                yield return null;
            }

            SoundManager.PlaySong("bedroom-boss");
            GlobalState.darkness.TargetAlpha(0.1f, 1.2f);
            GlobalState.screenShake.Shake(0.01f, 3f);

            while (GlobalState.screenShake.Active())
            {
                MathUtilities.MoveTo(ref GlobalState.PlayerLight.scale, 1f, 1.2f);
                yield return null;
            }

            foreach (var orb in _orbs)
            {
                orb.exists = true;
            }

            IEnumerator horizontal = MoveHorizontal();
            float y_timer = 0f;

            while (_health > 6)
            {
                y_timer += GameTimes.DeltaTime;
                Position.Y = _preset.Position.Y + 10 * MathF.Sin(y_timer);
                horizontal.MoveNext();
                RotateOrbs();
                yield return null;
            }

            IEnumerator stage2 = Stage2Logic();

            while (_health > 0)
            {
                stage2.MoveNext();
                yield return null;
            }

            GlobalState.darkness.TargetAlpha(1, 0.6f);

            while (!GlobalState.darkness.ReachedAlpha)
            {
                FadeEverything();
                yield return null;
            }

            GlobalState.Dialogue = Dialogue.DialogueManager.GetDialogue("sun_guy", "after_fight");

            while(!GlobalState.LastDialogueFinished)
            {
                FadeEverything();
                yield return null;
            }

            for(int i = 0; i < 3; ++i)
            {
                GlobalState.flash.Flash(2.38f,Color.White);
                GlobalState.screenShake.Shake(0.01f, 1f);
                SoundManager.PlaySoundEffect("sun_guy_death_short");
                while (GlobalState.flash.Active())
                    yield return null;
            }

            GlobalState.flash.Flash(3.3f, Color.White);
            GlobalState.screenShake.Shake(0.02f, 2f);
            SoundManager.PlaySoundEffect("sun_guy_death_long");
            while (GlobalState.flash.Active())
                yield return null;

            GlobalState.darkness.TargetAlpha(0.75f, 1.25f);

            while (!MathUtilities.MoveTo(ref GlobalState.PlayerLight.scale, 4f, 3.6f))
                yield return null;

            GlobalState.darkness.ForceAlpha(0.5f);
            GlobalState.events.BossDefeated.Add("BEDROOM");

            _preset.Alive = exists = false;

            foreach(var e in SubEntities())
            {
                e.exists = false;
            }

            SoundManager.PlaySong("bedroom");

            yield break;
        }

        void FadeEverything()
        {
            MathUtilities.MoveTo(ref opacity, 0, 3f);
            foreach(var orb in _orbs)
            {
                MathUtilities.MoveTo(ref orb.opacity, 0, 2f);
            }
            MathUtilities.MoveTo(ref wave.opacity, 0, 4f);
        }

        void RotateOrbs()
        {
            foreach (var orb in _orbs)
            {
                orb.angle += orb.rotation_speed * GameTimes.DeltaTime;
                orb.Position = Center + orb.radius * new Vector2(MathF.Cos(orb.angle), MathF.Sin(orb.angle));
            }
        }

        IEnumerator MoveHorizontal()
        {
            while (true)
            {
                while (!MathUtilities.MoveTo(ref Position.X, _preset.Position.X + 60, 50f))
                {
                    yield return null;
                }
                while (!MathUtilities.MoveTo(ref Position.X, _preset.Position.X - 50, 50f))
                {
                    yield return null;
                }
            }
        }

        IEnumerator Stage2Logic()
        {
            while (true)
            {
                //Get into position
                while (!(MathUtilities.MoveTo(ref Position.X, _preset.Position.X, 90f) && MathUtilities.MoveTo(ref Position.Y, _preset.Position.Y, 12f)))
                {
                    RotateOrbs();
                    yield return null;
                }
                //Bounce around dropping dust
                IEnumerator horizontal = MoveHorizontal();
                IEnumerator movement_logic = MoveVertical();
                while (movement_logic.MoveNext())
                {
                    foreach (var orb in _orbs)
                    {
                        MathUtilities.MoveTo(ref orb.radius, 8, 30f);
                        MathUtilities.MoveTo(ref orb.rotation_speed, 240f, 0.6f);
                    }
                    RotateOrbs();
                    horizontal.MoveNext();
                    yield return null;
                }
                //Charge up and shoot wave
                movement_logic = ChargeLogic();
                while (movement_logic.MoveNext())
                {
                    yield return null;
                }
            }
        }

        IEnumerator MoveVertical()
        {
            for (int i = 0; i <= dusts.Length; ++i)
            {
                while (!MathUtilities.MoveTo(ref Position.Y, _preset.Position.Y - 32, 120))
                    yield return null;

                while (!MathUtilities.MoveTo(ref Position.Y, _preset.Position.Y + 80f, 90))
                    yield return null;

                GlobalState.screenShake.Shake(0.05f, 0.3f);
                if (i < dusts.Length)
                {
                    Dust d = dusts[i];
                    d.Position = Position;
                    d.exists = true;
                    d.Play("unpoof");
                }

                SoundManager.PlaySoundEffect("hit_wall");
            }
            yield break;
        }


        IEnumerator ChargeLogic()
        {
            while (!(MathUtilities.MoveTo(ref Position.X, _preset.Position.X + 30, 60f) & MathUtilities.MoveTo(ref Position.Y, _preset.Position.Y - 20, 60f)))
            {
                MathUtilities.MoveTo(ref _orbs[0].Position.X, Position.X + 4, 120f);
                MathUtilities.MoveTo(ref _orbs[0].Position.Y, Position.Y + 10, 120f);
                MathUtilities.MoveTo(ref _orbs[1].Position.X, Position.X + -4, 120f);
                MathUtilities.MoveTo(ref _orbs[1].Position.Y, Position.Y + 10, 120f);
                yield return null;
            }

            SoundManager.PlaySoundEffect("sun_guy_charge");
            wave.exists = true;
            wave.End = false;
            wave.Position = _preset.Position - new Vector2(50, 10);
            wave.velocity = Vector2.Zero;
            wave.Play("play");
            wave.Flicker(3f);

            while (!(MathUtilities.MoveTo(ref _orbs[0].Position.X, wave.Position.X, 18f) & MathUtilities.MoveTo(ref _orbs[1].Position.X, _preset.Position.X + 75, 18f)))
            {
                GlobalState.screenShake.Shake(0.02f, 0.1f);

                if (wave.End) break;

                yield return null;
            }

            if (!wave.End)
                SoundManager.PlaySoundEffect("sun_guy_death_short");

            wave.velocity.Y = 30f;

            IEnumerator horizontal = MoveHorizontal();

            while (!wave.End)
            {
                horizontal.MoveNext();
                yield return null;
            }

            while (wave.exists)
                yield return null;

            foreach (Dust d in dusts)
            {
                d.Play("poof");
            }

            _player.broom.dust = null;

            while (dusts.Any((d) => d.exists))
            {
                yield return null;
            }

            yield break;
        }

        public override void Collided(Entity other)
        {
            if (other is Player p && opacity == 1f)
            {
                p.ReceiveDamage(1);
            }
            else if (other is Broom && !_flickering && _health > 0)
            {
                _health--;
                Flicker(1.5f);
                SoundManager.PlaySoundEffect("sun_guy_scream2");
            }
        }

        public override IEnumerable<Entity> SubEntities() => Enumerable.Concat<Entity>(_orbs, dusts).Concat(Enumerable.Repeat(wave, 1));

    }

    [Collision(typeof(Player))]
    class SeerOrb : Entity
    {
        Seer _parent;
        public float radius;
        public float rotation_speed;
        public float angle = 0f;

        public SeerOrb(int startframe, float radius, float speed, Seer parent) : base(parent.Position + Vector2.UnitX * radius, "light_orb", 16, 16, Drawing.DrawOrder.FG_SPRITES)
        {
            _parent = parent;
            exists = false;
            this.radius = radius;
            rotation_speed = speed;

            AddAnimation("glow", CreateAnimFrameArray(startframe, startframe + 1, startframe + 2, (startframe + 3) % 5, (startframe + 5) % 5), 10);
            Play("glow");

        }

        public override void Collided(Entity other)
        {
            if(opacity == 1f)
                (other as Player).ReceiveDamage(1);
        }

    }

    [Collision(typeof(Player), typeof(Dust))]
    class SeerWave : Entity
    {
        public bool End = false;

        public SeerWave() : base(Vector2.Zero, "sun_guy_wave", 128, 8, Drawing.DrawOrder.FG_SPRITES)
        {
            exists = false;
            AddAnimation("play", CreateAnimFrameArray(3, 4, 5), 8);
            AddAnimation("evaporate", CreateAnimFrameArray(2, 1, 0), 8, false);
            Play("play");
        }

        public override void Update()
        {
            base.Update();
            if (End && CurAnimFinished)
            {
                exists = false;
            }
            if (Position.Y > 160)
            {
                End = true;
                exists = false;
            }
        }

        public override void Collided(Entity other)
        {
            if (other is Player p && !End && opacity == 1f)
            {
                p.ReceiveDamage(1);
                if (velocity == Vector2.Zero)
                {
                    exists = false;
                }
            }
            Play("evaporate");
            End = true;
        }
    }
}
