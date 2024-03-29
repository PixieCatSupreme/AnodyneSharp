﻿using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using AnodyneSharp.UI;
using AnodyneSharp.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AnodyneSharp.States.CutsceneState;

namespace AnodyneSharp.Entities.Interactive
{
    public class HealthCicada : UIEntity
    {
        private const float Movement = 30f;
        private HealthCicadaSentinel _sentinel;
        private bool drawInGameCoordinates = true;
        private Vector2 gameOffset = MapUtilities.GetRoomUpperLeftPos(GlobalState.CurrentMapGrid) - Vector2.UnitY * 20;

        private EntityPreset _preset;

        private Player _player;

        private BoxFX[] _particles;

        IEnumerator<string> _state;

        private bool _chirp;

        public static AnimatedSpriteRenderer GetSprite(int f) => new("life_cicada", 16, 16,
            new Anim("idle", new int[] { f + 2, f + 3, f + 2, f + 3, f + 2, f + 2, f + 2, f + 2 },8),
            new Anim("fly", new int[] { f, f + 1 },14),
            new Anim("gnaw", new int[] { f + 2, f + 3 },14)
            );

        public HealthCicada(HealthCicadaSentinel parent, EntityPreset preset, Player player)
            : base(new Vector2(-16, 30), GetSprite(preset.Frame), DrawOrder.FG_SPRITES)
        {
            _sentinel = parent;

            _preset = preset;

            _player = player;

            _chirp = false;

            _state = StateLogic();

            _particles = new BoxFX[10];

            for (int i = 0; i < _particles.Length; i++)
            {
                _particles[i] = new BoxFX(i % 2 == 0);
            }
            visible = false;
        }

        public override void Update()
        {
            base.Update();

            if (_chirp)
            {
                SoundManager.PlaySoundEffect("cicada_chirp");
            }

            _state.MoveNext();

            foreach (var box in _particles)
            {
                if (box.exists)
                {
                    box.Update();
                }
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            foreach (var box in _particles)
            {
                if (box.exists)
                {
                    box.PostUpdate();
                    GlobalState.UIEntities.Add(box);
                }
            }
        }

        protected IEnumerator<string> StateLogic()
        {
            //s_invisible ctr -1
            while (GlobalState.IsDungeon && !GlobalState.events.BossDefeated.Contains(GlobalState.CURRENT_MAP_NAME))
            {
                yield return "WaitForBoss";
            }

            Vector2 targetPos = MapUtilities.GetInGridPosition(_sentinel.Position);

            visible = true;
            Play("fly");

            _chirp = true;

            //s_animating ctr -1
            while (!(MathUtilities.MoveTo(ref Position.X, targetPos.X, Movement) & MathUtilities.MoveTo(ref Position.Y, targetPos.Y + 20, Movement)))
            {
                yield return "FlyToStart";
            }

            _sentinel.Activated = true;
            Play("idle");

            //s_visible ctr 0
            while (!_sentinel.PlayerCollided)
            {
                if (_sentinel.PlayerAway)
                {

                    Play("fly");
                    velocity.Y = -100;

                    //s_visible crt 1
                    while (!MathUtilities.MoveTo(ref opacity, 0, 1.2f))
                    {
                        yield return "FlyAway";
                    }

                    _sentinel.exists = false;

                    yield break;
                }

                yield return "WaitForPlayer";
            }

            layer = DrawOrder.HEALTH_UPGRADE;
            drawInGameCoordinates = false;

            float timer = 0;

            GlobalState.StartCutscene = CoroutineUtils.WaitFor<CutsceneEvent>(()=>!_preset.Alive);

            Play("fly");

            //s_animating ctr 0
            while (GlobalState.CUR_HEALTH < GlobalState.MAX_HEALTH)
            {
                timer += GameTimes.DeltaTime;

                if (timer > 0.5f)
                {
                    timer = 0;
                    SoundManager.PlaySoundEffect("get_small_health");

                    GlobalState.CUR_HEALTH++;
                }

                yield return "HealPlayer";
            }

            targetPos = HealthBar.GetHealthPiecePos(GlobalState.MAX_HEALTH);
            _chirp = false;

            //s_animating ctr 1
            while (!(MathUtilities.MoveTo(ref Position.X, targetPos.X - 4, Movement) & MathUtilities.MoveTo(ref Position.Y, targetPos.Y - 4, Movement)))
            {
                yield return "FlyToHealth";
            }

            Play("gnaw");
            int gnaws = 0;

            //First one is instant
            timer = 2f;

            while (gnaws < 3)
            {
                timer += GameTimes.DeltaTime;

                if (timer > 1.7f)
                {
                    timer = 0;
                    gnaws++;
                    SoundManager.PlaySoundEffect("4sht_pop");
                    foreach (var box in _particles)
                    {
                        box.Spawn(Position);
                    }
                }

                yield return "Gnaw";
            }

            GlobalState.MAX_HEALTH++;
            GlobalState.CUR_HEALTH++;

            Play("fly");
            velocity.Y = -40;

            while (!MathUtilities.MoveTo(ref opacity, 0, 0.6f))
            {
                yield return "FadingOut";
            }

            _preset.Alive = false;

            while (_particles.Any(p => p.exists))
            {
                yield return "WaitingForBoxes";
            }

            _sentinel.exists = false;

            yield break;
        }

        public void RegisterDrawing()
        {
            if (drawInGameCoordinates)
            {
                Vector2 oldPos = Position;
                Position += gameOffset;
                DrawImpl();
                Position = oldPos;
            }
            else
            {
                GlobalState.UIEntities.Add(this);
            }
        }

        private class BoxFX : UIEntity
        {
            public BoxFX(bool isSmall) 
                : base(Vector2.Zero, "life_cicada_particle", 0, isSmall ? 3 : 4, isSmall ? 3 : 4, DrawOrder.EQUIPPED_BORDER)
            {
                exists = false;
            }

            public void Spawn(Vector2 pos)
            {
                exists = true;
                Position = pos + new Vector2(6);

                opacity = 1;
                acceleration.Y = GlobalState.RNG.Next(50, 71);

                velocity = new Vector2(GlobalState.RNG.Next(-40, 81), GlobalState.RNG.Next(10, 31));
            }

            public override void Update()
            {
                base.Update();

                if (MathUtilities.MoveTo(ref opacity, 0, 0.3f))
                {
                    exists = false;
                }
            }
        }
    }
}
