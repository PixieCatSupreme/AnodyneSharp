﻿using AnodyneSharp.Drawing;
using AnodyneSharp.Entities.Base.Rendering;
using AnodyneSharp.Entities.Gadget;
using AnodyneSharp.FSM;
using AnodyneSharp.Registry;
using AnodyneSharp.Sounds;
using Microsoft.Xna.Framework;
using RSG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnodyneSharp.Entities.Enemy.Redcave
{
    [NamedEntity, Enemy, Collision(typeof(Player), MapCollision = true, KeepOnScreen = true)]
    public class Mover : Entity
    {
        private class MoveState : TimerState
        {
            public MoveState()
            {
                AddTimer(1f, "MoveTimer");
            }
        }

        EntityPreset _preset;
        Player _player;

        private IState _state;

        public static AnimatedSpriteRenderer GetSprite() => new("f_mover", 16, 16,
            new Anim("Foom", new int[] { 0, 1 },4, false),
            new Anim("Die", new int[] { 0, 1, 2, 1, 2, 1, 2 },12,false)
            );

        public Mover(EntityPreset preset, Player player)
             : base(preset.Position, GetSprite(), DrawOrder.ENTITIES)
        {
            _preset = preset;

            _player = player;

            drag = new Vector2(100);


            _state = new StateMachineBuilder()
                .State<MoveState>("Foom")
                    .Event("MoveTimer", (state) =>
                    {
                        SoundManager.PlaySoundEffect("mover_move");

                        Play("Foom");

                        Vector2 direction = _player.Position - Position;

                        float normal = MathF.Min(3, 100 / direction.Length());

                        velocity = direction * normal + new Vector2(GlobalState.RNG.Next(-5, 0), GlobalState.RNG.Next(-5, 0));
                    })
                    .Event<CollisionEvent<Player>>("Player", (state, p) =>
                    {
                        p.entity.additionalVel = velocity;
                    })
                .End()
                .State("Die")
                    .Enter((state) =>
                    {
                        _preset.Alive = false;

                        velocity = Vector2.Zero;
                        Play("Die");
                    })
                    .Condition(() => AnimFinished, (state) =>
                    {
                        exists = false;

                        SoundManager.PlaySoundEffect("mover_die");

                        GlobalState.SpawnEntity(new Explosion(this));
                    })
                .End()
                .Build();

            _state.ChangeState("Foom");

        }

        public override void Update()
        {
            base.Update();

            _state.Update(GameTimes.DeltaTime);
        }

        public override void Collided(Entity other)
        {
            if (other is Player p)
            {
                _state.TriggerEvent("Player", new CollisionEvent<Player>() { entity = p });
            }
        }

        internal void Die()
        {
            if (_preset.Alive)
            {
                _state.ChangeState("Die");
            }
        }
    }
}
